$.fn.dataTable.moment("M/D/YY h:mm:ss A");
$.fn.dataTable.moment("M/D/YYYY h:mm:ss A");

$(document).ready(function () {
    $("#my_projects").DataTable({
        dom: 'Blfrtip',
        buttons: [
            'colvis'
        ],
        responsive: true
    });
    $("#all_projects").DataTable({
        dom: 'Blfrtip',
        buttons: [
            'colvis'
        ],
        responsive: true
    });
    $("#project_tickets").DataTable({
        dom: 'Blfrtip',
        buttons: [
            'colvis'
        ],
        responsive: true
    });
    $("#my_tickets").DataTable({
        dom: 'Blfrtip',
        buttons: [
            'colvis'
        ],
        responsive: true
    });
    $("#sub_tickets").DataTable({
        dom: 'Blfrtip',
        buttons: [
            'colvis'
        ],
        responsive: true
    });

});