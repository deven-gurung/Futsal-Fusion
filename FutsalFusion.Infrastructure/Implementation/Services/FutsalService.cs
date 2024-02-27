using FutsalFusion.Application.DTOs.Futsal;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Application.Interfaces.Services;
using FutsalFusion.Domain.Constants;
using FutsalFusion.Domain.Entities;
using FutsalFusion.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace FutsalFusion.Infrastructure.Implementation.Services;

public class FutsalService : IFutsalService
{
    private readonly UserManager<User> _userManager;
    private readonly IGenericRepository _genericRepository;
    private readonly IFileUploadService _fileUploadService;

    public FutsalService(UserManager<User> userManager, IGenericRepository genericRepository, IFileUploadService fileUploadService)
    {
        _userManager = userManager;
        _genericRepository = genericRepository;
        _fileUploadService = fileUploadService;
    }

    public List<FutsalResponseDto> Get()
    {
        var futsals = _genericRepository.Get<Futsal>(x => x.IsActive);

        var result = (from futsal in futsals
                                            let futsalImage = _genericRepository.GetFirstOrDefault<FutsalImage>(x => 
                                                x.FutsalId == futsal.Id)
                                            let owner = _genericRepository.GetById<User>(futsal.FutsalOwnerId)
                                            select new FutsalResponseDto()
                                            {
                                                FutsalId = futsal.Id,
                                                FutsalName = futsal.Name,
                                                FutsalImage = futsalImage.ImageURL,
                                                FutsalPhrase = futsal.Phrase,
                                                FutsalLocationAddress = $"{futsal.LocationAddress} {futsal.City}",
                                                OwnerEmail = owner.Email,
                                                OwnerName = owner.Name,
                                            }).ToList();

        return result;
    }

    public FutsalDetailResponseDto GetById(Guid futsalId)
    {
        var futsal = _genericRepository.GetById<Futsal>(futsalId);

        var futsalImages = _genericRepository.Get<FutsalImage>(x => x.FutsalId == futsal.Id);

        var futsalCourts = _genericRepository.Get<Court>(x => x.FutsalId == futsal.Id);

        var user = _genericRepository.GetById<User>(futsal.FutsalOwnerId);

        var result = new FutsalDetailResponseDto()
        {
            FutsalId = futsal.Id,
            FutsalName = futsal.Name,
            FutsalPhrase = futsal.Phrase,
            OwnerEmail = user.Email,
            OwnerName = user.Name,
            FutsalLocationAddress = futsal.LocationAddress,
            Images = futsalImages.Select(x => x.ImageURL).ToList(),
            Courts = futsalCourts.Select(x => new CourtResponseDto()
            {
                Type = x.Type,
                Title = x.Title,
                Description = x.Description,
                Images = _genericRepository.Get<CourtImage>(y => y.CourtId == x.Id).Select(z => z.ImageURL).ToList()
            }).ToList()
        };

        return result;
    }

    public async Task Upsert(FutsalRequestDto futsal)
    {
        if (futsal.Id == Guid.Empty)
        {
            var owner = new User()
            {
                Email = futsal.OwnerEmail,
                Name = futsal.OwnerName,
                EmailConfirmed = true,
                NormalizedEmail = futsal.OwnerEmail.ToUpper(),
                UserName = futsal.OwnerEmail,
                NormalizedUserName = futsal.OwnerEmail.ToUpper(),
            };
            
            await _userManager.CreateAsync(owner, Constants.Passwords.FutsalPassword);
            
            await _userManager.AddToRoleAsync(owner, Constants.Roles.Futsal);

            var user = _genericRepository.GetFirstOrDefault<User>(x => x.Email == futsal.OwnerEmail);

            var futsalModel = new Futsal()
            {
                Name = futsal.FutsalName,
                Phrase = futsal.FutsalPhrase,
                City = futsal.FutsalCity,
                LocationAddress = futsal.FutsalLocationAddress,
                Description = futsal.FutsalDescription,
                CreatedBy = futsal.CreatedBy,
                FutsalOwnerId = user.Id
            };

            var futsalId = _genericRepository.Insert(futsalModel);

            foreach (var futsalImageModel in futsal.Images.Select(futsalImage => 
                         _fileUploadService.UploadDocument(Constants.FilePath.FutsalImagesFilePath, futsalImage))
                         .Select(uploadedImageURL => new FutsalImage()
                     {
                         FutsalId = futsalId,
                         CreatedBy = futsal.CreatedBy,
                         ImageURL = uploadedImageURL,
                         ImageType = 1
                     }))
            {
                _genericRepository.Insert(futsalImageModel);
            }

            foreach (var courtImageModel in from futsalCourt in futsal.Courts let futsalCourtModel = new Court()
                     {
                         FutsalId = futsalId,
                         Description = futsalCourt.Description,
                         Title = futsalCourt.Title,
                         Type = futsalCourt.Type,
                         CreatedBy = futsal.CreatedBy
                     } 
                     let courtId = _genericRepository.Insert(futsalCourtModel) 
                                        from courtImageModel in futsalCourt.Images.Select(futsalCourtImage => 
                                                _fileUploadService.UploadDocument(Constants.FilePath.CourtImagesFilePath, futsalCourtImage))
                                        .Select(courtImageURL => new CourtImage()
                                        {
                                             CourtId = courtId,
                                             ImageType = 1,
                                             ImageURL = courtImageURL,
                                             CreatedBy = futsal.CreatedBy
                                        }) select courtImageModel)
            {
                _genericRepository.Insert(courtImageModel);
            }
        }
    }
}