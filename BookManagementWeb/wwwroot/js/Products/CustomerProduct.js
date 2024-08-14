var elementItemListGroup = document.getElementsByClassName("list-group-item");
var elementRange = document.querySelector("input[type='range']")
var debounceTimeout;

// Tạo hiệu ứng khi chạy
RenderFadeout('.product-item');

function getSliderRangerPrice() {
    let rangersPrice = [];
    let minPrice = $("#slider-range").slider("values", 0);
    let maxPrice = $("#slider-range").slider("values", 1);
    rangersPrice.push(minPrice)
    rangersPrice.push(maxPrice)
    return rangersPrice;
}

function getSelectedCategoryIds() {
    let selectedCategoryIds = [];
    $('input[name="SelectedCategoryIds"]:checked').each(function () {
        selectedCategoryIds.push($(this).val());
    });
    return selectedCategoryIds;
}
function RemoveCategory(categoryIdRemove, element) {
    const _this = element;
    // Bỏ check category
    $('input[name="SelectedCategoryIds"]:checked').each(function () {
        if ($(this).val() == categoryIdRemove) {
            $(this).prop('checked', false);
            sendData();
        }
    });
}
function getSearchString() {
    let searchString = document.getElementById('SearchInput').value;
    return searchString;
}

function RenderFadeout(elementClass) {
    $(elementClass).each(function (index) {
        $(this).delay(index * 100).queue(function (next) {
            $(this).addClass('show');
            next();
        });
    });
}


// Hàm gửi dữ liệu qua AJAX
function sendData() {
    $.ajax({
        url: '/Customer/Product/GetProductListByFilter',
        type: 'POST',
        data: {
            selectedCategoryIds: getSelectedCategoryIds(),
            SearchInput: getSearchString(),
            RangerPrice: getSliderRangerPrice()
        },
        success: function (result) {
            // Xóa các partial cũ
            $('#partial-background').nextAll().remove();
            // Show partial sau khi filter
            $('#partial-background').after(result);
            // Áp dụng hiệu ứng mượt mà cho các sản phẩm mới
            RenderFadeout('.category-item')
            RenderFadeout('.product-item')
        },
        error: function (xhr, status, error) {
            console.error('Có lỗi xảy ra');
        }
    });
}

//#region HANDLE EVENT

//Xử lý thanh kéo khoảng tiền
$("#slider-range").slider({
    range: true,
    min: 0,
    max: 100,
    values: [0, 100],
    slide: function (event, ui) {
        $("#minPrice").html(ui.values[0] + "$");
        $("#maxPrice").html(ui.values[1] + "$");
        clearTimeout(debounceTimeout);
        debounceTimeout = setTimeout(function () {
            sendData();
        }, 1000);
    }
});
// Khởi tạo range input Jquery
$("#minPrice").html($("#slider-range").slider("values", 0) + "$");
$("#maxPrice").html($("#slider-range").slider("values", 1) + "$");

// Xử lý nhập vào input
document.getElementById('SearchInput').addEventListener("input", (e) => {
    clearTimeout(debounceTimeout);
    debounceTimeout = setTimeout(function () {
        sendData();
    }, 1000);
})

// Xử lý nhấn checkbox
$('input[name="SelectedCategoryIds"]').on('change', function () {
    clearTimeout(debounceTimeout);
    debounceTimeout = setTimeout(function () {
        sendData();
    }, 1000);
});

// Xử lý thanh kéo giới hạn mức tiền
//elementRange.addEventListener("change", (e) => {
//    // Xoa bo class active cu
//    Array.from(elementItemListGroup).forEach((e) => {
//        e.classList.remove("fw-bold", "text-secondary")
//    });

//    if (e.target.value == 0) {
//        elementItemListGroup[0].classList.add("fw-bold", "text-secondary")
//    } else if (e.target.value == 50) {
//        elementItemListGroup[1].classList.add("fw-bold", "text-secondary")
//    } else {
//        elementItemListGroup[2].classList.add("fw-bold", "text-secondary")
//    }
//})

//#endregion