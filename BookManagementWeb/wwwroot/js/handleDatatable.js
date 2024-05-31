$(document).ready(function () {
    $('#myTable').DataTable();
});
function DeleteConfirm(url, element) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: (data) => {
                    // Xóa hàng từ DataTable và DOM
                    var table = $('#myTable').DataTable();
                    table.row($(element).parents('tr'))
                        .remove()
                        .draw();
                    Swal.fire({
                        title: "Deleted!",
                        text: data.message,
                        icon: "success"
                    });
                }
            })
        }
    });
}