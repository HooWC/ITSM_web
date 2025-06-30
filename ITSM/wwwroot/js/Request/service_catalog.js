// Grid View and List View toggle
const gridBtn = document.querySelector(".ser-cat-grid");
const listBtn = document.querySelector(".ser-cat-list");

if (gridBtn && listBtn) {
    gridBtn.addEventListener("click", function () {
        listBtn.classList.remove("ser-cat-active");
        gridBtn.classList.add("ser-cat-active");
        document.querySelector(".ser-cat-products-area-wrapper").classList.add("ser-cat-gridView");
        document.querySelector(".ser-cat-products-area-wrapper").classList.remove("ser-cat-tableView");
    });

    listBtn.addEventListener("click", function () {
        listBtn.classList.add("ser-cat-active");
        gridBtn.classList.remove("ser-cat-active");
        document.querySelector(".ser-cat-products-area-wrapper").classList.remove("ser-cat-gridView");
        document.querySelector(".ser-cat-products-area-wrapper").classList.add("ser-cat-tableView");
    });
}

$('#refreshButton').click(function () {
    location.reload();
});

function filterProducts() {
    const searchText = document.querySelector(".ser-cat-search-bar").value.toLowerCase().trim();
    
    const productRows = document.querySelectorAll(".ser-cat-products-row");

    productRows.forEach(row => {
        const productName = row.querySelector(".ser-cat-product-cell.ser-cat-image span").textContent.toLowerCase();
        const categoryId = row.getAttribute("data-category-id");
        const status = row.getAttribute("data-status");

        let searchMatch = true;
        let categoryMatch = true;
        let statusMatch = true;

        // Search for matches
        if (searchText !== "") {
            searchMatch = productName.includes(searchText);
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