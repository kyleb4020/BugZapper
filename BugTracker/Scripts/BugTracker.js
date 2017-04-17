function reloadNotification() {
    $("#notice_partial").load("_Notifications.cshtml")
};

function glow() {
    setTimeout(function () {
        if ($('.light').hasClass("dim")) {
            $('.light').removeClass('dim');
            $('.light').addClass('bright');
            glow();
        }
        else
            if ($('.light').hasClass("bright")) {
                $('.light').removeClass('bright');
                $('.light').addClass('dim');
                glow();
            }
            else {
                glow();
            }
    }, 1000);
};

$.ajaxSetup({
    // Disable caching of AJAX responses
    cache: false
});

$(document).ready(function () {
    $(".cage").hover(function () {
        $(".light").removeClass("dim", "bright");
        $(".light").addClass("zap");
    });
    $(".cage").mouseleave(function () {
        $(".light").removeClass("zap");
        $(".light").addClass("dim");
    });
    glow();
    $("#Projects").multiselect();
    //$("#UnProjects").multiselect();
    $("#Users").multiselect();
    $("#Roles").multiselect();
    //$("#UnUsers").multiselect();
    //$("#UnRoles").multiselect();
    $("#Tickets").multiselect();
    $("#Tickets2").multiselect();
    //$("#UnTickets").multiselect();
    $("#Developers").multiselect();
    //$("#UnDevelopers").multiselect();
    $("#Types").multiselect();
    $("#PMId").multiselect();
    $("#StatusId").multiselect();
    $("#PriorityId").multiselect();

    $("#comments-title").click(function () {
        $("#comments-list").animate({
            height: "toggle"
        });
    });
    $("#attachments-title").click(function () {
        $("#attachments-list").animate({
            height: "toggle"
        });
    });
    $("#history-title").click(function () {
        $("#history-list").animate({
            height: "toggle"
        });
    });
    
    //$("#notice_partial").load("/Views/Shared/_Notifications.cshtml", function () {
    //    alert("Load was performed.");
    //});
    var num = 0;
    $("#add_attachment").click(function () {
        $("#add_attachment_form").append('<label class="control-label col-md-2">Upload Attachment</label><div class="col-md-10"><input name="upload[' + num + ']" type="file" class="form-control" id="fileUpload" /><p class="text-primary" style="font-weight:bold">Accepted file types: .pdf .doc .docx .zip .txt .rtf .jpg .jpeg .png .gif</p></div><label class="control-label col-md-2">Attachment Description</label><div class="col-md-10"><textarea class="form-control" name="AttachmentDescription"></textarea></div>')
        num++;
    });
    
    $(".side-nav li").on("click", function () {
        $(".side-nav li").removeClass("selected");
        $(this).addClass("selected");
    });
    if (top.location.pathname === '/Home/Index') {
        $(".side-nav li").removeClass("selected");
        $("#home-nav").addClass("selected");
    }
    if (top.location.pathname === '/Home/') {
        $(".side-nav li").removeClass("selected");
        $("#home-nav").addClass("selected");
    }
    if (top.location.pathname === '/') {
        $(".side-nav li").removeClass("selected");
        $("#home-nav").addClass("selected");
    }
    if (top.location.pathname === '/Admin/') {
        $(".side-nav li").removeClass("selected");
        $("#admin-nav").addClass("selected open");
        $("#admin-dash").addClass("selected");
    }
    if (top.location.pathname === '/Admin/Index') {
        $(".side-nav li").removeClass("selected");
        $("#admin-nav").addClass("selected open");
        $("#admin-dash").addClass("selected");
    }
    if (top.location.pathname === '/Admin/Roles') {
        $(".side-nav li").removeClass("selected");
        $("#admin-nav").addClass("selected open");
        $("#admin-roles").addClass("selected");
    }
    if (top.location.pathname === '/Admin/Projects') {
        $(".side-nav li").removeClass("selected");
        $("#admin-nav").addClass("selected open");
        $("#admin-projects").addClass("selected");
    }
    if (top.location.pathname === '/Projects/') {
        $(".side-nav li").removeClass("selected");
        $("#projects-nav").addClass("selected open");
        $("#projects-dash").addClass("selected");
    }
    if (top.location.pathname === '/Projects/Create') {
        $(".side-nav li").removeClass("selected");
        $("#projects-nav").addClass("selected open");
        $("#projects-create").addClass("selected");
    }
    if (top.location.pathname === '/Projects/IndexEdit') {
        $(".side-nav li").removeClass("selected");
        $("#projects-nav").addClass("selected open");
        $("#projects-edit").addClass("selected");
    }
    if (top.location.pathname === '/Tickets/Index') {
        $(".side-nav li").removeClass("selected");
        $("#tickets-nav").addClass("selected open");
        $("#tickets-dash").addClass("selected");
    }
    if (top.location.pathname === '/Tickets/') {
        $(".side-nav li").removeClass("selected");
        $("#tickets-nav").addClass("selected open");
        $("#tickets-dash").addClass("selected");
    }
    if (top.location.pathname === '/Tickets/Create') {
        $(".side-nav li").removeClass("selected");
        $("#tickets-nav").addClass("selected open");
        $("#tickets-create").addClass("selected");
    }
    if (top.location.pathname === '/Tickets/Edit') {
        $(".side-nav li").removeClass("selected");
        $("#tickets-nav").addClass("selected open");
        $("#tickets-edit").addClass("selected");
    }


    $.fn.dataTable.moment('D/M/YYYY h:mm:ss a');
    $("#my_projects").DataTable({
        dom: 'Blfrtip',
        buttons: [
            'colvis'
        ],
        responsive: true
    });
    $.fn.dataTable.moment('D/M/YYYY h:mm:ss a');
    $("#all_projects").DataTable({
        dom: 'Blfrtip',
        buttons: [
            'colvis'
        ],
        responsive: true
    });
    $.fn.dataTable.moment('D/M/YYYY h:mm:ss a');
    $("#project_tickets").DataTable({
        dom: 'Blfrtip',
        buttons: [
            'colvis'
        ],
        responsive: true
    });
    $.fn.dataTable.moment('D/M/YYYY h:mm:ss a');
    $("#my_tickets").DataTable({
        dom: 'Blfrtip',
        buttons: [
            'colvis'
        ],
        responsive: true
    });
    $.fn.dataTable.moment('D/M/YYYY h:mm:ss a');
    $("#sub_tickets").DataTable({
        dom: 'Blfrtip',
        buttons: [
            'colvis'
        ],
        responsive: true
    });
});