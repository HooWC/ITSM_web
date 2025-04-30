$(function () {
    $('#log-outBtn').on('click', function () {
        $.ajax({
            url: '/Ajax/_Logout',
            method: 'GET',
            success: function (response) {
                if (response.success) {
                    // Redirect to the login page after successful logout
                    window.location.href = '/Auth/Login';
                } else {
                    console.error('Login Failed:', response.message);
                }
            },
            error: function (xhr, status, error) {
                console.error('Login Error:', error);
            }
        });
    });
})