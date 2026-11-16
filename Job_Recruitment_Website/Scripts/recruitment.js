/**
 * D-JOBS Recruitment Agency - Main Layout & Validation Script
 * File: ~/Scripts/recruitment.js
 */

document.addEventListener("DOMContentLoaded", function () {
    console.log("D-JOBS Layout Script Loaded Successfully.");

    // =========================================================================
    // 1. TỰ ĐỘNG ẨN THÔNG BÁO (ALERT) SAU 4 GIÂY
    // (Hỗ trợ cả .alert-dismissible lẫn .alert-success thông thường trong các View)
    // =========================================================================
    const alerts = document.querySelectorAll('.alert-dismissible, .alert-success');
    alerts.forEach(function (alertBox) {
        setTimeout(function () {
            if (!alertBox || !document.body.contains(alertBox)) return;

            if (alertBox.classList.contains('alert-dismissible') && typeof bootstrap !== 'undefined') {
                let bsAlert = bootstrap.Alert.getOrCreateInstance(alertBox);
                bsAlert.close();
            } else {
                alertBox.style.transition = 'opacity 0.5s ease';
                alertBox.style.opacity = '0';
                setTimeout(function () {
                    if (alertBox.parentNode) alertBox.parentNode.removeChild(alertBox);
                }, 500);
            }
        }, 4000);
    });


    // =========================================================================
    // 2. HIỆU ỨNG SMOOTH SCROLL CHO SIDEBAR TRÊN MOBILE
    // =========================================================================
    if (window.innerWidth < 768) {
        const activeFilter = document.querySelector('.job-filter .aside-item.active');
        if (activeFilter) {
            activeFilter.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }
    }


    // =========================================================================
    // 3. HIỆU ỨNG CHUYỂN TRANG MƯỢT MÀ KHI CLICK BỘ LỌC HOẶC MENU
    // =========================================================================
    const filterItems = document.querySelectorAll('.job-filter .aside-item, nav .nav-link');
    filterItems.forEach(function (item) {
        item.addEventListener('click', function () {
            const href = this.getAttribute('href');
            if (href && href !== '#' && !href.startsWith('javascript')) {
                document.body.style.opacity = '0.85';
                document.body.style.transition = 'opacity 0.2s ease';
            }
        });
    });

    // Khôi phục lại độ sáng trang nếu người dùng bấm nút Back trên trình duyệt
    window.addEventListener('pageshow', function () {
        document.body.style.opacity = '1';
    });


    // =========================================================================
    // 4. TẠO NÚT "QUAY LẠI ĐẦU TRANG" (BACK TO TOP) ĐỘNG
    // =========================================================================
    const backToTopBtn = document.createElement('button');
    backToTopBtn.type = 'button';
    backToTopBtn.innerHTML = '<i class="bi bi-arrow-up"></i>';
    backToTopBtn.className = 'btn btn-primary-custom position-fixed shadow rounded-circle d-none align-items-center justify-content-center';
    backToTopBtn.style.cssText = 'bottom: 30px; right: 30px; width: 45px; height: 45px; z-index: 1050; transition: all 0.3s ease;';
    backToTopBtn.setAttribute('title', 'Lên đầu trang');
    document.body.appendChild(backToTopBtn);

    window.addEventListener('scroll', function () {
        if (window.pageYOffset > 300) {
            backToTopBtn.classList.remove('d-none');
            backToTopBtn.classList.add('d-flex');
        } else {
            backToTopBtn.classList.remove('d-flex');
            backToTopBtn.classList.add('d-none');
        }
    });

    backToTopBtn.addEventListener('click', function () {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });


    // =========================================================================
    // 5. HỘP THOẠI XÁC NHẬN (ĐĂNG XUẤT, ĐÓNG TIN, TỪ CHỐI HỒ SƠ, KHÓA TÀI KHOẢN)
    // =========================================================================
    // 5.1 Xác nhận Đăng xuất
    const logoutLinks = document.querySelectorAll('a[href*="Logout"]');
    logoutLinks.forEach(function (link) {
        link.addEventListener('click', function (e) {
            if (!confirm("Bạn có chắc chắn muốn đăng xuất khỏi hệ thống không?")) {
                e.preventDefault();
                document.body.style.opacity = '1';
            }
        });
    });

    // 5.2 Xác nhận Đóng tin tuyển dụng (Jobs/MyJobs)
    const closeJobBtns = document.querySelectorAll('.btn-close-job');
    closeJobBtns.forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            if (!confirm("Bạn có chắc chắn muốn đóng tin tuyển dụng này không?")) {
                e.preventDefault();
            }
        });
    });

    // 5.3 Xác nhận Từ chối ứng viên (Applications/Candidates)
    const statusForms = document.querySelectorAll('form[action*="UpdateStatus"]');
    statusForms.forEach(function (form) {
        form.addEventListener('submit', function (e) {
            const statusInput = form.querySelector('input[name="newStatus"]');
            if (statusInput && statusInput.value === 'Rejected') {
                if (!confirm("Bạn có chắc chắn muốn TỪ CHỐI hồ sơ ứng viên này không?")) {
                    e.preventDefault();
                }
            }
        });
    });

    // 5.4 Xác nhận Khóa / Mở khóa tài khoản (Admin/Index)
    const toggleUserForms = document.querySelectorAll('form[action*="ToggleUserStatus"]');
    toggleUserForms.forEach(function (form) {
        form.addEventListener('submit', function (e) {
            const btn = form.querySelector('button[type="submit"]');
            const actionText = btn ? btn.innerText.trim().toLowerCase() : 'thay đổi trạng thái';
            if (!confirm("Bạn có chắc chắn muốn " + actionText + " tài khoản này không?")) {
                e.preventDefault();
            }
        });
    });


    // =========================================================================
    // 6. KÍCH HOẠT BOOTSTRAP TOOLTIPS
    // =========================================================================
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }


    // =========================================================================
    // 7. HIỆU ỨNG ĐẾM SỐ LIỆU (COUNTER ANIMATION) CHO CÁC BADGE/COUNT
    // =========================================================================
    const countElements = document.querySelectorAll('.aside-count');
    countElements.forEach(function (el) {
        const target = parseInt(el.innerText, 10);
        if (!isNaN(target) && target > 0) {
            let current = 0;
            const increment = Math.ceil(target / 20);
            const timer = setInterval(function () {
                current += increment;
                if (current >= target) {
                    el.innerText = target;
                    clearInterval(timer);
                } else {
                    el.innerText = current;
                }
            }, 30);
        }
    });


    // =========================================================================
    // 8. KIỂM TRA FILE UPLOAD (CV PDF & LOGO CÔNG TY) NGAY TRÊN TRÌNH DUYỆT
    // =========================================================================
    // 8.1 Kiểm tra file CV (.pdf, tối đa 5MB) tại trang Apply
    const cvInput = document.querySelector('input[type="file"][name="cvFile"]');
    if (cvInput) {
        cvInput.addEventListener('change', function () {
            const file = this.files[0];
            if (!file) return;

            const fileName = file.name.toLowerCase();
            const maxSize = 5 * 1024 * 1024; // 5MB

            if (!fileName.endsWith('.pdf')) {
                alert("Hệ thống chỉ chấp nhận file CV định dạng PDF (.pdf)!");
                this.value = '';
                this.classList.add('is-invalid');
                return;
            }
            if (file.size > maxSize) {
                alert("Dung lượng file CV không được vượt quá 5MB!");
                this.value = '';
                this.classList.add('is-invalid');
                return;
            }
            this.classList.remove('is-invalid');
        });
    }

    // 8.2 Kiểm tra & Xem trước ảnh Logo công ty (.jpg, .jpeg, .png)
    const logoInput = document.querySelector('input[type="file"][name="logoFile"]');
    if (logoInput) {
        logoInput.addEventListener('change', function () {
            const file = this.files[0];
            if (!file) return;

            const validExts = ['.jpg', '.jpeg', '.png'];
            const fileName = file.name.toLowerCase();
            const isValid = validExts.some(function (ext) { return fileName.endsWith(ext); });

            if (!isValid) {
                alert("Vui lòng chọn file ảnh Logo định dạng .jpg, .jpeg hoặc .png!");
                this.value = '';
                this.classList.add('is-invalid');
                return;
            }

            this.classList.remove('is-invalid');
            let previewImg = document.getElementById('logo-preview-img');
            if (!previewImg) {
                previewImg = document.createElement('img');
                previewImg.id = 'logo-preview-img';
                previewImg.style.cssText = 'max-height: 100px; margin-top: 10px; border-radius: 8px; border: 1px solid #e6e9ef; padding: 4px; display: block;';
                logoInput.parentNode.appendChild(previewImg);
            }
            previewImg.src = URL.createObjectURL(file);
        });
    }


    // =========================================================================
    // 9. TỰ ĐỘNG KIỂM TRA INPUT TRỐNG KHI SUBMIT FORM (CLIENT-SIDE VALIDATION)
    // =========================================================================
    const forms = document.querySelectorAll('form');
    forms.forEach(function (form) {
        // Tự động gỡ viền đỏ ngay khi người dùng bắt đầu gõ lại vào ô
        const requiredInputs = form.querySelectorAll('[required]');
        requiredInputs.forEach(function (input) {
            input.addEventListener('input', function () {
                if (this.value.trim()) {
                    this.classList.remove('is-invalid');
                }
            });
        });

        form.addEventListener('submit', function (e) {
            let isValid = true;

            requiredInputs.forEach(function (input) {
                // Với input type="file" thì kiểm tra files.length thay vì .value.trim()
                const isEmpty = input.type === 'file'
                    ? (!input.files || input.files.length === 0)
                    : !input.value.trim();

                if (isEmpty) {
                    isValid = false;
                    input.classList.add('is-invalid');
                } else {
                    input.classList.remove('is-invalid');
                }
            });

            if (!isValid) {
                e.preventDefault();
                const firstInvalid = form.querySelector('.is-invalid');
                if (firstInvalid) {
                    firstInvalid.focus();
                    firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
            }
        });
    });
});