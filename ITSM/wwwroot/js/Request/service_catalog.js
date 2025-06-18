// Grid View and List View toggle
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

// Refresh button
$('#refreshButton').click(function () {
    location.reload();
});