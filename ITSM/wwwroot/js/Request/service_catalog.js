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

// 搜索功能
document.querySelector(".ser-cat-search-bar").addEventListener("input", function () {
    const searchText = this.value.toLowerCase().trim();
    const productRows = document.querySelectorAll(".ser-cat-products-row");

    productRows.forEach(row => {
        const productName = row.querySelector(".ser-cat-product-cell.ser-cat-image span").textContent.toLowerCase();
        if (productName.includes(searchText)) {
            row.style.display = "";
        } else {
            row.style.display = "none";
        }
    });
});

// 筛选功能
document.querySelector(".ser-cat-filter-button.ser-cat-apply").addEventListener("click", function () {
    const categoryValue = document.querySelector(".ser-cat-filter-menu select:nth-child(2)").value;
    const statusValue = document.querySelector(".ser-cat-filter-menu select:nth-child(4)").value;
    const productRows = document.querySelectorAll(".ser-cat-products-row");

    productRows.forEach(row => {
        const category = row.querySelector(".ser-cat-product-cell.ser-cat-category").textContent.replace("Category:", "").trim();
        const status = row.querySelector(".ser-cat-status").textContent.trim();

        let categoryMatch = true;
        let statusMatch = true;

        if (categoryValue !== "All Categories") {
            categoryMatch = category === categoryValue;
        }

        if (statusValue !== "All Status") {
            statusMatch = status === statusValue;
        }

        if (categoryMatch && statusMatch) {
            row.style.display = "";
        } else {
            row.style.display = "none";
        }
    });

    document.querySelector(".ser-cat-filter-menu").classList.remove("ser-cat-active");
});

// 重置筛选
document.querySelector(".ser-cat-filter-button.ser-cat-reset").addEventListener("click", function () {
    document.querySelectorAll(".ser-cat-filter-menu select").forEach(select => {
        select.selectedIndex = 0;
    });

    document.querySelectorAll(".ser-cat-products-row").forEach(row => {
        row.style.display = "";
    });

    document.querySelector(".ser-cat-filter-menu").classList.remove("ser-cat-active");
});