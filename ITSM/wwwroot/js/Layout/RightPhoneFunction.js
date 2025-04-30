$(function () {
    $('#log-outBtn').on('click', function () {
        $.ajax({
            url: '/Ajax/_Logout',
            method: 'GET',
            success: function (response) {
                if (response.success) {
                    // 退出成功后重定向到登录页面
                    window.location.href = '/Auth/Login';
                } else {
                    console.error('退出登录失败:', response.message);
                }
            },
            error: function (xhr, status, error) {
                console.error('退出登录请求出错:', error);
            }
        });
    });
})