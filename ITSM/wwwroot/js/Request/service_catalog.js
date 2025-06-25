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
