function commonMessage(messageType, message){
    if (messageType === "success") {
        Swal.fire({
            icon: 'success',
            title: 'Success',
            html: '<strong>Message:</strong> ' + message,
            confirmButtonText: 'Ok',
            confirmButtonColor: '#DD5471',
            buttonsStyling: false,
            customClass: {
                popup: 'swal2-popup',
                confirmButton: 'swal2-button btn btn-danger mb-3 text-white'
            }
        });
    }
    else if (messageType === "warning") {
        Swal.fire({
            icon: 'warning',
            title: 'Warning',
            html: '<strong>Message:</strong> ' + message,
            confirmButtonText: 'Ok',
            confirmButtonColor: '#DD5471',
            buttonsStyling: false,
            customClass: {
                popup: 'swal2-popup',
                confirmButton: 'swal2-button btn btn-danger mb-3 text-white'
            }
        });
    }
    else if (messageType === "error") {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            html: '<strong>Message:</strong> ' + message,
            confirmButtonText: 'Ok',
            confirmButtonColor: '#DD5471',
            buttonsStyling: false,
            customClass: {
                popup: 'swal2-popup',
                confirmButton: 'swal2-button btn btn-danger mb-3 text-white'
            }
        });
    }
}

$(document).ready(function() {
    $('[data-provide="datepicker"]').datepicker({
        format: 'd-M-yyyy',
        autoclose: true
    });
});

function OnlyNumber(evt) {
    let ASCIICode= (evt.which) ? evt.which : evt.keyCode

    return !(ASCIICode > 31 && (ASCIICode < 48 || ASCIICode > 57));
}

function DecimalNumberOnly(evt) {
    evt = (evt) ? evt : window.event;

    let charCode = (evt.which) ? evt.which : evt.keyCode;

    if (charCode === 8 || charCode === 37) {
        return true;
    } else if (charCode === 46 && evt.path[0].value.indexOf('.') !== -1) {
        return false;
    } else if (charCode > 31 && charCode !== 46 && charCode !== 44 && (charCode < 48 || charCode > 57)) {
        return false;
    }

    return true;
}