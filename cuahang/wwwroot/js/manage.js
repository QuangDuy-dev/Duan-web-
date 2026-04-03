$(document).ready(function () {

    var editUrl = $("#productForm").data("edit-url");
    var createUrl = $("#productForm").data("create-url");

    var mode = "add";

    $("#searchBox").on("keyup", function () {
        var value = $(this).val().toLowerCase();
        $("#productTable tbody tr").filter(function () {
            $(this).toggle($(this).find(".ten").text().toLowerCase().includes(value));
        });
    });

    var $mota = $("#inputMoTa");

    $mota.on("input", function () { autoResize(this); });
    $mota.on("keydown", function (e) {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            handleAction();
        }
    });

    function autoResize(el) {
        el.style.height = "auto";
        el.style.height = el.scrollHeight + "px";
    }

    function setEditMode(id) {
        mode = "edit";
        $("#editId").val(id);
        $("#formTitle").html('<i class="bi bi-pencil"></i> Chỉnh sửa sản phẩm — ID: ' + id);
        $("#btnAction")
            .removeClass("btn-success")
            .addClass("btn-warning")
            .html('<i class="bi bi-pencil-square"></i> Cập nhật sản phẩm');
        $("#btnReset").removeClass("d-none");
    }

    function setAddMode() {
        mode = "add";
        $("#editId").val("0");
        $("#formTitle").html('<i class="bi bi-plus-circle"></i> Thêm sản phẩm');
        $("#btnAction")
            .removeClass("btn-warning")
            .addClass("btn-success")
            .html('<i class="bi bi-plus-lg"></i> Thêm sản phẩm');
        $("#btnReset").addClass("d-none");
    }

    $(document).on("click", ".product-row", function () {
        var $row = $(this);

        $(".product-row").removeClass("table-warning");
        $row.addClass("table-warning");

        $("#inputTenSP").val($row.data("ten"));
        $("#inputGia").val($row.data("gia"));
        $("#inputSoLuong").val($row.data("soluong"));
        $("#inputImageUrl").val($row.data("imageurl"));
        $("#inputLoaiSp").val($row.data("loai"));
        $("#inputMoTa").val($row.data("mota"));
        autoResize(document.getElementById("inputMoTa"));

        setEditMode($row.data("id"));

        $("html, body").animate({ scrollTop: $(".card").offset().top - 20 }, 300);
    });

    $("#btnReset").on("click", function () {
        resetForm();
    });

    function resetForm() {
        $("#productForm")[0].reset();
        $(".product-row").removeClass("table-warning");
        autoResize(document.getElementById("inputMoTa"));
        setAddMode();
    }

    $("#btnAction").on("click", function () {
        handleAction();
    });

    function handleAction() {
        if (mode === "edit") {
            doUpdate();
        } else {
            $("#productForm").attr("action", createUrl).submit();
        }
    }

    function doUpdate() {
        var id = $("#editId").val();
        if (!id || id === "0") return;

        var payload = {
            Id: id,
            TenSP: $("#inputTenSP").val(),
            Gia: parseFloat($("#inputGia").val()),
            SoLuongTon: parseInt($("#inputSoLuong").val()),
            ImageUrl: $("#inputImageUrl").val(),
            LoaiSp: $("#inputLoaiSp").val(),
            MoTa: $("#inputMoTa").val()
        };

        $.ajax({
            url: editUrl,
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                if (res.success) {
                    var $row = $(".product-row[data-id='" + id + "']");

                    $row.data("ten", payload.TenSP);
                    $row.data("gia", payload.Gia);
                    $row.data("soluong", payload.SoLuongTon);
                    $row.data("imageurl", payload.ImageUrl);
                    $row.data("loai", payload.LoaiSp);
                    $row.data("mota", payload.MoTa);

                    $row.find(".ten").text(payload.TenSP);
                    $row.find("td:eq(2)").text(payload.Gia.toLocaleString("vi-VN"));
                    $row.find("td:eq(3)").text(payload.SoLuongTon);
                    $row.find("td:eq(4)").text(payload.LoaiSp);
                    $row.find("img").attr("src", "/image/" + payload.ImageUrl);

                    resetForm();

                    $row.addClass("table-success");
                    setTimeout(function () { $row.removeClass("table-success"); }, 1500);
                } else {
                    alert("Cập nhật thất bại: " + (res.message || "Lỗi không xác định"));
                }
            },
            error: function () {
                alert("Lỗi kết nối, vui lòng thử lại.");
            }
        });
    }

});

document.querySelectorAll('.btn-details').forEach(function (btn) {
    btn.addEventListener('click', function () {
        document.getElementById('detailTen').textContent = this.dataset.ten;
        document.getElementById('detailGia').textContent = this.dataset.gia;
        document.getElementById('detailSoLuong').textContent = this.dataset.soluong;
        document.getElementById('detailMoTa').textContent = this.dataset.mota || '(Không có mô tả)';
        document.getElementById('detailImg').src = '/image/' + this.dataset.imageurl;

        var modal = new bootstrap.Modal(document.getElementById('detailsModal'));
        modal.show();
    });
});