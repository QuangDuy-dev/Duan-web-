$(document).ready(function () {
    var editUrl = $("#productForm").data("edit-url");
    var createUrl = $("#productForm").data("create-url");
    var placeholderPreview = "https://placehold.co/720x520/e2e8f0/475569?text=Preview";
    var selectedPreviewUrl = null;
    var defaultUploadText = "Chưa chọn ảnh nào";
    var mode = "add";

    function syncSidebarActiveState() {
        var currentPath = window.location.pathname.toLowerCase();
        var bestMatch = null;
        var bestLength = -1;

        $(".sidebar-nav a").each(function () {
            var linkPath = new URL(this.href, window.location.origin).pathname.toLowerCase();
            var isMatch = currentPath === linkPath || (linkPath !== "/" && currentPath.indexOf(linkPath) === 0);

            $(this).removeClass("active");

            if (isMatch && linkPath.length > bestLength) {
                bestMatch = this;
                bestLength = linkPath.length;
            }
        });

        if (bestMatch) {
            $(bestMatch).addClass("active");
        }
    }

    function isMobileViewport() {
        return window.matchMedia("(max-width: 991.98px)").matches;
    }

    function closeMobileMenu() {
        $("body").removeClass("mobile-menu-open");
        $("#mobileMenuToggle").attr("aria-expanded", "false");
    }

    function toggleMobileMenu() {
        var willOpen = !$("body").hasClass("mobile-menu-open");
        $("body").toggleClass("mobile-menu-open", willOpen);
        $("#mobileMenuToggle").attr("aria-expanded", willOpen ? "true" : "false");
    }

    function showManageAlert(message, title) {
        var modalElement = document.getElementById("manageAlertModal");
        if (!modalElement) {
            alert(message);
            return;
        }

        document.getElementById("manageAlertMessage").textContent = message;
        document.getElementById("manageAlertModalLabel").textContent = title || "Kiểm tra lại thông tin";
        bootstrap.Modal.getOrCreateInstance(modalElement).show();
    }

    function autoResize(el) {
        if (!el) return;
        el.style.height = "auto";
        el.style.height = el.scrollHeight + "px";
    }

    function formatCurrency(value) {
        var amount = parseFloat(value);
        if (isNaN(amount)) return "0 đ";
        return amount.toLocaleString("vi-VN") + " đ";
    }

    function getImagePath(fileName) {
        if (!fileName) return placeholderPreview;

        var trimmed = String(fileName).trim();
        if (!trimmed) return placeholderPreview;
        if (/^https?:\/\//i.test(trimmed)) return trimmed;

        return "/image/" + trimmed;
    }

    function revokeSelectedPreview() {
        if (selectedPreviewUrl) {
            URL.revokeObjectURL(selectedPreviewUrl);
            selectedPreviewUrl = null;
        }
    }

    function updateUploadFileName() {
        var imageInput = document.getElementById("inputImageFile");
        var label = document.getElementById("imageUploadFilename");
        if (!label) return;

        if (imageInput && imageInput.files && imageInput.files[0]) {
            label.textContent = imageInput.files[0].name;
            return;
        }

        label.textContent = defaultUploadText;
    }

    function updatePreview() {
        if (!$("#previewName").length) return;

        var name = $("#inputTenSP").val().trim();
        var price = $("#inputGia").val();
        var stock = $("#inputSoLuong").val();
        var category = $("#inputLoaiSp").val().trim();
        var currentImageUrl = $("#currentImageUrl").val().trim();
        var imageInput = document.getElementById("inputImageFile");
        var description = $("#inputMoTa").val().trim();
        var previewSource = getImagePath(currentImageUrl);

        revokeSelectedPreview();

        if (imageInput && imageInput.files && imageInput.files[0]) {
            selectedPreviewUrl = URL.createObjectURL(imageInput.files[0]);
            previewSource = selectedPreviewUrl;
        }

        $("#previewName").text(name || "Tên sản phẩm sẽ hiển thị ở đây");
        $("#previewPrice").text(formatCurrency(price));
        $("#previewStock").text("Số lượng tồn: " + (stock || "0"));
        $("#previewCategory").text(category || "Chưa phân loại");
        $("#previewDescription").text(description || "Mô tả ngắn của sản phẩm sẽ được cập nhật theo nội dung bạn nhập trong biểu mẫu.");
        $("#formPreviewImg").attr("src", previewSource || placeholderPreview);
    }

    function setEditMode(id) {
        mode = "edit";
        $("#editId").val(id);
        $("#formTitle").html('<i class="bi bi-pencil"></i> Chỉnh sửa sản phẩm #' + id);
        $("#btnAction")
            .removeClass("btn-success")
            .addClass("btn-warning")
            .html('<i class="bi bi-pencil-square"></i> Cập nhật sản phẩm');
        $("#btnReset").removeClass("d-none");
    }

    function setAddMode() {
        mode = "add";
        $("#editId").val("0");
        $("#currentImageUrl").val("");
        $("#formTitle").html('<i class="bi bi-plus-circle"></i> Thêm sản phẩm mới');
        $("#btnAction")
            .removeClass("btn-warning")
            .addClass("btn-success")
            .html('<i class="bi bi-plus-lg"></i> Thêm sản phẩm');
        $("#btnReset").addClass("d-none");
    }

    function resetForm() {
        $("#productForm")[0].reset();
        $(".product-row").removeClass("row-active");
        $("#currentImageUrl").val("");
        revokeSelectedPreview();
        updateUploadFileName();
        autoResize(document.getElementById("inputMoTa"));
        setAddMode();
        updatePreview();
    }

    function validateNonNegativeFields() {
        var priceValue = parseFloat($("#inputGia").val());
        var stockValue = parseInt($("#inputSoLuong").val(), 10);

        if (!isNaN(priceValue) && priceValue < 0) {
            showManageAlert("Giá sản phẩm không được là số âm.");
            $("#inputGia").focus();
            return false;
        }

        if (!isNaN(stockValue) && stockValue < 0) {
            showManageAlert("Số lượng tồn không được là số âm.");
            $("#inputSoLuong").focus();
            return false;
        }

        return true;
    }

    function handleAction() {
        if (!validateNonNegativeFields()) {
            return;
        }

        if (mode === "edit") {
            doUpdate();
        } else {
            $("#productForm").attr("action", createUrl).submit();
        }
    }

    function doUpdate() {
        var id = $("#editId").val();
        if (!id || id === "0") return;

        var tenValue = $("#inputTenSP").val();
        var giaValue = parseFloat($("#inputGia").val());
        var soLuongValue = parseInt($("#inputSoLuong").val(), 10);
        var loaiValue = $("#inputLoaiSp").val();
        var moTaValue = $("#inputMoTa").val();
        var formData = new FormData($("#productForm")[0]);

        formData.set("Id", id);
        formData.set("TenSP", tenValue);
        formData.set("Gia", $("#inputGia").val());
        formData.set("SoLuongTon", $("#inputSoLuong").val());
        formData.set("LoaiSp", loaiValue);
        formData.set("MoTa", moTaValue);
        formData.set("ImageUrl", $("#currentImageUrl").val());

        $.ajax({
            url: editUrl,
            method: "POST",
            processData: false,
            contentType: false,
            data: formData,
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                if (res.success) {
                    var savedImageUrl = res.imageUrl || $("#currentImageUrl").val();
                    var $row = $(".product-row[data-id='" + id + "']");

                    $row.data("ten", tenValue);
                    $row.data("gia", giaValue);
                    $row.data("soluong", soLuongValue);
                    $row.data("imageurl", savedImageUrl);
                    $row.data("loai", loaiValue);
                    $row.data("mota", moTaValue);

                    $row.find(".ten").text(tenValue);
                    $row.find("td:eq(2)").text(giaValue.toLocaleString("vi-VN") + " đ");
                    $row.find("td:eq(3)").text(soLuongValue);
                    $row.find(".category-pill").text(loaiValue || "Chưa rõ");
                    $row.find("img").attr("src", getImagePath(savedImageUrl));

                    resetForm();

                    $row.addClass("table-success");
                    setTimeout(function () {
                        $row.removeClass("table-success");
                    }, 1500);
                } else {
                    showManageAlert("Cập nhật thất bại: " + (res.message || "Lỗi không xác định"), "Không thể cập nhật");
                }
            },
            error: function () {
                showManageAlert("Lỗi kết nối, vui lòng thử lại.", "Lỗi kết nối");
            }
        });
    }

    $("#searchBox").on("keyup", function () {
        var value = $(this).val().toLowerCase();
        var visibleRows = 0;

        $("#productTable tbody tr").each(function () {
            var isMatch = $(this).find(".ten").text().toLowerCase().includes(value);
            $(this).toggle(isMatch);

            if (isMatch) {
                visibleRows += 1;
            }
        });

        $("#emptySearchState").toggleClass("d-none", visibleRows > 0);
    });

    var $mota = $("#inputMoTa");
    $mota.on("input", function () { autoResize(this); });
    $mota.on("keydown", function (e) {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            handleAction();
        }
    });

    $("#inputTenSP, #inputGia, #inputSoLuong, #inputLoaiSp, #inputMoTa").on("input", function () {
        updatePreview();
    });

    $("#inputImageFile").on("change", function () {
        updateUploadFileName();
        updatePreview();
    });

    $(document).on("click", ".product-row", function () {
        var $row = $(this);

        $(".product-row").removeClass("row-active");
        $row.addClass("row-active");

        $("#inputTenSP").val($row.data("ten"));
        $("#inputGia").val($row.data("gia"));
        $("#inputSoLuong").val($row.data("soluong"));
        $("#currentImageUrl").val($row.data("imageurl"));
        $("#inputImageFile").val("");
        updateUploadFileName();
        $("#inputLoaiSp").val($row.data("loai"));
        $("#inputMoTa").val($row.data("mota"));

        autoResize(document.getElementById("inputMoTa"));
        updatePreview();
        setEditMode($row.data("id"));

        $("html, body").animate({ scrollTop: $("#productForm").offset().top - 120 }, 300);
    });

    $("#btnReset").on("click", function () {
        resetForm();
    });

    $("#btnAction").on("click", function () {
        handleAction();
    });

    $(".btn-details").each(function () {
        this.addEventListener("click", function () {
            document.getElementById("detailTen").textContent = this.dataset.ten;
            document.getElementById("detailGia").textContent = this.dataset.gia;
            document.getElementById("detailSoLuong").textContent = this.dataset.soluong;
            document.getElementById("detailMoTa").textContent = this.dataset.mota || "(Không có mô tả)";
            document.getElementById("detailImg").src = getImagePath(this.dataset.imageurl);

            var modal = new bootstrap.Modal(document.getElementById("detailsModal"));
            modal.show();
        });
    });

    $(".btn-delete-product").on("click", function (event) {
        event.preventDefault();

        var productName = $(this).data("product-name") || "sản phẩm này";
        var deleteUrl = $(this).attr("href");
        var modalElement = document.getElementById("deleteProductModal");

        $("#deleteProductMessage").text('Bạn có chắc muốn xóa "' + productName + '" không?');
        $("#confirmDeleteProduct").attr("href", deleteUrl);

        bootstrap.Modal.getOrCreateInstance(modalElement).show();
    });

    $("#mobileMenuToggle").on("click", function () {
        toggleMobileMenu();
    });

    $("#sidebarBackdrop").on("click", function () {
        closeMobileMenu();
    });

    $(".sidebar-nav a, .sidebar-exit").on("click", function () {
        if (isMobileViewport()) {
            closeMobileMenu();
        }
    });

    $(window).on("resize", function () {
        if (!isMobileViewport()) {
            closeMobileMenu();
        }
    });

    syncSidebarActiveState();
    closeMobileMenu();
    updateUploadFileName();
    autoResize(document.getElementById("inputMoTa"));
    updatePreview();
});
