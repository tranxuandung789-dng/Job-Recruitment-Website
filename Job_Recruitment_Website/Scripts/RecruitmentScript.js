/**
 * D-JOBS Recruitment Agency - Main Layout Script
 * File: ~/Scripts/RecruitmentScript.js
 */

document.addEventListener("DOMContentLoaded", function () {
    console.log("D-JOBS Layout Script Loaded Successfully.");

    // 1. Tự động ẩn các thông báo alert (nếu có trong view con) sau 4 giây
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            let bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 4000);
    });

    // 2. Hiệu ứng active linh hoạt cho các bộ lọc sidebar hoặc menu điều hướng (nếu cần xử lý động bằng JS)
    const filterItems = document.querySelectorAll('.job-filter .aside-item');
    filterItems.forEach(item => {
        item.addEventListener('click', function () {
            // Có thể thêm hiệu ứng loading nhỏ khi người dùng click đổi danh mục hoặc địa điểm
            // Không bắt buộc nhưng giúp trải nghiệm mượt mà hơn
        });
    });

    // 3. Xử lý tối ưu hiển thị trên thiết bị di động (nếu thu nhỏ màn hình)
    // Ví dụ: Tự động cuộn mượt khi chọn filter trên màn hình nhỏ
    if (window.innerWidth < 768) {
        const activeFilter = document.querySelector('.job-filter .aside-item.active');
        if (activeFilter) {
            activeFilter.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }
    }
});