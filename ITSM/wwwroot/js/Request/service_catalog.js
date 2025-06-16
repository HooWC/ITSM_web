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

function filterProducts() {
    const searchText = document.querySelector(".ser-cat-search-bar").value.toLowerCase().trim();
    const categorySelect = document.querySelector(".ser-cat-category-filter");
    const statusSelect = document.querySelector(".ser-cat-status-filter");
    
    const categoryValue = categorySelect.value;
    const statusValue = statusSelect.value;
    
    console.log("Filter criteria:", {
        searchText: searchText,
        categoryValue: categoryValue,
        statusValue: statusValue
    });
    
    const productRows = document.querySelectorAll(".ser-cat-products-row");

    productRows.forEach(row => {
        const productName = row.querySelector(".ser-cat-product-cell.ser-cat-image span").textContent.toLowerCase();
        const categoryId = row.getAttribute("data-category-id");
        const status = row.getAttribute("data-status");
        
        console.log("Product attributes:", {
            productName: productName,
            categoryId: categoryId,
            status: status
        });

        let searchMatch = true;
        let categoryMatch = true;
        let statusMatch = true;

        // Search for matches
        if (searchText !== "") {
            searchMatch = productName.includes(searchText);
        }

        // Classification matching
        if (categoryValue !== "all") {
            categoryMatch = categoryId === categoryValue;
        }

        // status match
        if (statusValue !== "all") {
            statusMatch = status === statusValue;
        }

        // Combine all filter conditions
        if (searchMatch && categoryMatch && statusMatch) {
            row.style.display = "";
        } else {
            row.style.display = "none";
        }
    });
}

// Search function
document.querySelector(".ser-cat-search-bar").addEventListener("input", function () {
    filterProducts();
});

// Filter function
document.querySelector(".ser-cat-filter-button.ser-cat-apply").addEventListener("click", function () {
    filterProducts();
    document.querySelector(".ser-cat-filter-menu").classList.remove("ser-cat-active");
});

// Reset filter
document.querySelector(".ser-cat-filter-button.ser-cat-reset").addEventListener("click", function () {
    document.querySelector(".ser-cat-category-filter").value = "all";
    document.querySelector(".ser-cat-status-filter").value = "all";
    
    filterProducts();
    document.querySelector(".ser-cat-filter-menu").classList.remove("ser-cat-active");
});

$('#refreshButton').click(function () {
    location.reload();
});