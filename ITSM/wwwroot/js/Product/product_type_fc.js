$('#Product_menu_create').addClass("active");

function handleProductTypeChange() {
    if ($('#product_type').val() === 'Service') {
        $('#quantity').val(1);
        $('#quantity').attr('readonly', true);
        $('#quantity').attr('min', 1);
        $('#quantity').attr('max', 1);
    } else {
        $('#quantity').attr('readonly', false);
        $('#quantity').attr('min', 1);
        $('#quantity').removeAttr('max');
    }
}
handleProductTypeChange();
$('#product_type').on('change', handleProductTypeChange);