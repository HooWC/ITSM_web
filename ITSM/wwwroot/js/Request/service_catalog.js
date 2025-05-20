document.querySelector(".ser-cat-jsFilter").addEventListener("click", function () {
    document.querySelector(".ser-cat-filter-menu").classList.toggle("ser-cat-active");
});

document.querySelector(".ser-cat-grid").addEventListener("click", function () {
    document.querySelector(".ser-cat-list").classList.remove("ser-cat-active");
    document.querySelector(".ser-cat-grid").classList.add("ser-cat-active");
    document.querySelector(".ser-cat-products-area-wrapper").classList.add("ser-cat-gridView");
    document
        .querySelector(".ser-cat-products-area-wrapper")
        .classList.remove("ser-cat-tableView");
});

document.querySelector(".ser-cat-list").addEventListener("click", function () {
    document.querySelector(".ser-cat-list").classList.add("ser-cat-active");
    document.querySelector(".ser-cat-grid").classList.remove("ser-cat-active");
    document.querySelector(".ser-cat-products-area-wrapper").classList.remove("ser-cat-gridView");
    document.querySelector(".ser-cat-products-area-wrapper").classList.add("ser-cat-tableView");
});

// 综合筛选函数
function filterProducts() {
    const searchText = document.querySelector(".ser-cat-search-bar").value.toLowerCase().trim();
    const categorySelect = document.querySelector(".ser-cat-category-filter");
    const statusSelect = document.querySelector(".ser-cat-status-filter");
    
    const categoryValue = categorySelect.value;
    const statusValue = statusSelect.value;
    
    console.log("筛选条件:", {
        searchText: searchText,
        categoryValue: categoryValue,
        statusValue: statusValue
    });
    
    const productRows = document.querySelectorAll(".ser-cat-products-row");

    productRows.forEach(row => {
        const productName = row.querySelector(".ser-cat-product-cell.ser-cat-image span").textContent.toLowerCase();
        const categoryId = row.getAttribute("data-category-id");
        const status = row.getAttribute("data-status");
        
        console.log("产品属性:", {
            productName: productName,
            categoryId: categoryId,
            status: status
        });

        let searchMatch = true;
        let categoryMatch = true;
        let statusMatch = true;

        // 搜索匹配
        if (searchText !== "") {
            searchMatch = productName.includes(searchText);
        }

        // 分类匹配
        if (categoryValue !== "all") {
            categoryMatch = categoryId === categoryValue;
        }

        // 状态匹配
        if (statusValue !== "all") {
            statusMatch = status === statusValue;
        }

        // 综合所有筛选条件
        if (searchMatch && categoryMatch && statusMatch) {
            row.style.display = "";
        } else {
            row.style.display = "none";
        }
    });
}

// 搜索功能
document.querySelector(".ser-cat-search-bar").addEventListener("input", function () {
    filterProducts();
});

// 筛选功能
document.querySelector(".ser-cat-filter-button.ser-cat-apply").addEventListener("click", function () {
    filterProducts();
    document.querySelector(".ser-cat-filter-menu").classList.remove("ser-cat-active");
});

// 重置筛选
document.querySelector(".ser-cat-filter-button.ser-cat-reset").addEventListener("click", function () {
    document.querySelector(".ser-cat-category-filter").value = "all";
    document.querySelector(".ser-cat-status-filter").value = "all";
    
    filterProducts();
    document.querySelector(".ser-cat-filter-menu").classList.remove("ser-cat-active");
});

$('#refreshButton').click(function () {
    location.reload();
});