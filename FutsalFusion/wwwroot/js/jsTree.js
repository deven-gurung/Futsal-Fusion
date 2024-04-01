$.fn.extend({
    treed: function (o) {
        var openedClass = 'mdi mdi-arrow-down-drop-circle';
        var closedClass = ' mdi mdi-arrow-right-drop-circle';

        if (typeof o != 'undefined') {
            if (typeof o.openedClass != 'undefined') {
                openedClass = o.openedClass;
            }
            if (typeof o.closedClass != 'undefined') {
                closedClass = o.closedClass;
            }
        };

        //initialize each of the top levels
        var tree = $(this);
        tree.addClass("tree");
        tree.find('li').has("ul").each(function () {
            var branch = $(this); //li with children ul
            branch.prepend("<i class='indicator mdi " + closedClass + "' style='font-size: 18px'></i>");
            branch.addClass('branch');
            branch.on('click', function (e) {
                if (this == e.target) {
                    var icon = $(this).children('i:first');
                    icon.toggleClass(openedClass + " " + closedClass);
                    $(this).children().children().toggle();
                }
            })
            branch.children().children().toggle();
        });
        //fire event from the dynamically added icon
        tree.find('.branch .indicator').each(function () {
            $(this).on('click', function () {
                $(this).closest('li').click();
            });
        });
        //fire event to open branch if the li contains an anchor instead of text
        tree.find('.branch>a').each(function () {
            $(this).on('click', function (e) {
                $(this).closest('li').click();
                e.preventDefault();
            });
        });
        //fire event to open branch if the li contains a button instead of text
        tree.find('.branch>button').each(function () {
            $(this).on('click', function (e) {
                $(this).closest('li').click();
                e.preventDefault();
            });
        });
    }
});

//Initialization of treeviews

$('.tree1').treed();
$('.tree1 .branch').each(function () {
    var icon = $(this).children('i:first');
    icon.toggleClass('mdi mdi-arrow-down-drop-circle  mdi mdi-arrow-right-drop-circle');
    $(this).children().children().toggle();
});

$('.tree2').treed();
$('.tree2 .branch').each(function () {
    var icon = $(this).children('i:first');
    icon.toggleClass('mdi mdi-arrow-down-drop-circle mdi mdi-arrow-right-drop-circle');
   $(this).children().children().toggle();
});


//check uncheck all values ( changed by Sujeet G on 03, Jul 2019 )
$(".parent").click(function () {
    //var themeID = $(this).attr('themeID');
    
    if ($(this).is(':checked')) {
        var parentli = $(this).parents('li').attr('id');
        $('#' + parentli).find('.child').prop('checked', true);
        $('#' + parentli).find('.subChild').prop('checked', true);
        $('#' + parentli).find('.subsubChild').prop('checked', true);
    }
    else {
        var parentli = $(this).parents('li').attr('id');
        $('#' + parentli).find('.child').prop('checked', false);
        $('#' + parentli).find('.subChild').prop('checked', false);
        $('#' + parentli).find('.subsubChild').prop('checked', false);
    }
});

$(".child").click(function () {
    // var x = 0; 
    if ($(this).is(':checked')) {
        $(this).parent('li').find('ul').find('.subsubChild').prop("checked", true);
        //  $(this).find('.subsubChild').prop("checked", true);
    } else {
        $(this).parent('li').find('ul').find('.subsubChild').prop("checked", false);
        $(".parent").prop("checked", false);
    }
    var noOfUncheckedChildCheckboxes = $('.child').not(':checked').length;
    if (noOfUncheckedChildCheckboxes == 0) { $(".parent").prop("checked", true); }

});

$(".subChild").click(function () {

    var x = 0;
    $(this).parent().parent().parent().parent().find("li .subChild").each(function () {
        if (!$(this).is(':checked')) {
            x++;
        }
        if (x != 0) {
            $(this).parent().parent().parent().parent().parent().find(".parent").prop('checked', false);
            $(this).parent().parent().parent().find(".child").prop('checked', false);
            $(this).parent().find("ul li input").prop('checked', false);
        }
        else {
            $(this).parent().parent().parent().parent().parent().find(".parent").prop('checked', true);
            $(this).parent().parent().parent().find(".child").prop('checked', true);
            $(this).parent().find("ul li input").prop('checked', true);
        }
    });

});

$(".subsubChild").click(function () {
    var x = 0;
    $(this).parent().parent().parent().parent().parent().parent().find("li .subsubChild").each(function () {
        if (!$(this).is(':checked')) {
            x++;
        }
    });
    if (x > 0) {
        $(".parent").prop("checked", false);
    }
    var noOfUncheckedSubChildCheckboxes = $('.subsubChild').not(':checked').length;
    if (noOfUncheckedSubChildCheckboxes == 0) { $(".parent").prop("checked", true); }

});

/// New Treeview

$(function () {

    $('input[type="checkbox"]').change(checkboxChanged);

    function checkboxChanged() {
        var $this = $(this),
        checked = $this.prop("checked"),
        container = $this.parent(),
        siblings = container.siblings();
        container.find('input[type="checkbox"]:enabled').prop({
            indeterminate: false,
            checked: checked
        })
    .siblings('label')
    .removeClass('custom-checked custom-unchecked custom-indeterminate')
    .addClass(checked ? 'custom-checked' : 'custom-unchecked');
    checkSiblings(container, checked);
    }

    function checkSiblings($el, checked) {
        var parent = $el.parent().parent(),
        all = true,
        indeterminate = false;

        $el.siblings().each(function () {
            return all = ($(this).children('input[type="checkbox"]').prop("checked") === checked);
        });

        if (all && checked) {
            parent.children('input[type="checkbox"]')
      .prop({
          indeterminate: false,
          checked: checked
      })
      .siblings('label')
      .removeClass('custom-checked custom-unchecked custom-indeterminate')
      .addClass(checked ? 'custom-checked' : 'custom-unchecked');

            checkSiblings(parent, checked);
        }
        else if (all && !checked) {
            indeterminate = parent.find('input[type="checkbox"]:checked').length > 0;
            parent.children('input[type="checkbox"]')
          .prop("checked", checked)
          .prop("indeterminate", indeterminate)
          .siblings('label')
          .removeClass('custom-checked custom-unchecked custom-indeterminate')
          .addClass(indeterminate ? 'custom-indeterminate' : (checked ? 'custom-checked' : 'custom-unchecked'));

            checkSiblings(parent, checked);
        }
        else {
            $el.parents("li").children('input[type="checkbox"]')
      .prop({
          indeterminate: true,
          checked: false
      })
      .siblings('label')
      .removeClass('custom-checked custom-unchecked custom-indeterminate')
      .addClass('custom-indeterminate');
        }
    }
});