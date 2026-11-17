/**
 * D-JOBS Recruitment Agency - Main Layout & Custom Modal Script
 * File: ~/Scripts/recruitment.js
 */

document.addEventListener("DOMContentLoaded", function () {
    console.log("D-JOBS Layout Script Loaded Successfully.");

    // =========================================================================
    // 0. KHỞI TẠO HỘP THOẠI MODAL ĐẸP (THAY THẾ CONFIRM / ALERT MẶC ĐỊNH)
    // =========================================================================
    const modalHtml = `
    <div class="modal fade" id="djobsCustomModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered" style="max-width: 420px;">
            <div class="modal-content border-0 shadow-lg" style="border-radius: 16px; overflow: hidden;">
                <div class="modal-body p-4 text-center">
                    <div id="djobsModalIcon" class="mx-auto mb-3 d-flex align-items-center justify-content-center rounded-circle"
                         style="width: 60px; height: 60px; background-color: #fff0dd; color: #f7901e; font-size: 28px;">
                        <i class="bi bi-question-circle-fill"></i>
                    </div>
                    <h5 id="djobsModalTitle" class="fw-bold text-dark mb-2">Xác nhận thao tác</h5>
                    <p id="djobsModalMessage" class="text-secondary small mb-4" style="line-height: 1.6; white-space: pre-line;"></p>
                    <div class="d-flex justify-content-center gap-2">
                        <button type="button" id="djobsModalCancel" class="btn btn-light border fw-semibold px-4 py-2 rounded-3" data-bs-dismiss="modal">
                            Hủy bỏ
                        </button>
                        <button type="button" id="djobsModalConfirm" class="btn text-white fw-bold px-4 py-2 rounded-3 shadow-sm"
                                style="background-color: #f7901e; border: none;">
                            Đồng ý
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>`;
    document.body.insertAdjacentHTML('beforeend', modalHtml);

    const modalEl = document.getElementById('djobsCustomModal');
    const bsModal = typeof bootstrap !== 'undefined' ? new bootstrap.Modal(modalEl) : null;
    const iconEl = document.getElementById('djobsModalIcon');
    const titleEl = document.getElementById('djobsModalTitle');
    const msgEl = document.getElementById('djobsModalMessage');
    const cancelBtn = document.getElementById('djobsModalCancel');
    const confirmBtn = document.getElementById('djobsModalConfirm');

    function showConfirmModal(options) {
        if (!bsModal) {
            if (confirm(options.message)) options.onConfirm();
            return;
        }
        titleEl.innerText = options.title || "Xác nhận";
        msgEl.innerText = options.message || "Bạn có chắc chắn muốn thực hiện thao tác này?";
        cancelBtn.style.display = "inline-block";
        confirmBtn.innerText = options.confirmText || "Đồng ý";

        if (options.type === "danger") {
            iconEl.style.backgroundColor = "#fee2e2";
            iconEl.style.color = "#dc2626";
            iconEl.innerHTML = '<i class="bi bi-exclamation-triangle-fill"></i>';
            confirmBtn.style.backgroundColor = "#dc2626";
        } else {
            iconEl.style.backgroundColor = "#fff0dd";
            iconEl.style.color = "#f7901e";
            iconEl.innerHTML = '<i class="bi bi-question-circle-fill"></i>';
            confirmBtn.style.backgroundColor = "#f7901e";
        }

        confirmBtn.onclick = function () {
            bsModal.hide();
            if (typeof options.onConfirm === "function") options.onConfirm();
        };

        bsModal.show();
    }

    function showInfoModal(title, message, isWarning) {
        if (!bsModal) {
            alert(message);
            return;
        }
        titleEl.innerText = title || "Thông báo";
        msgEl.innerText = message || "";
        cancelBtn.style.display = "none";
        confirmBtn.innerText = "Đóng";

        if (isWarning) {
            iconEl.style.backgroundColor = "#fee2e2";
            iconEl.style.color = "#dc2626";
            iconEl.innerHTML = '<i class="bi bi-exclamation-circle-fill"></i>';
            confirmBtn.style.backgroundColor = "#dc2626";
        } else {
            iconEl.style.backgroundColor = "#e0f2fe";
            iconEl.style.color = "#0284c7";
            iconEl.innerHTML = '<i class="bi bi-envelope-paper-fill"></i>';
            confirmBtn.style.backgroundColor = "#f7901e";
        }

        confirmBtn.onclick = function () {
            bsModal.hide();
        };

        bsModal.show();
    }


    // =========================================================================
    // 1. PHÂN TRANG MENU SIDEBAR KIỂU TOPCV (5 MỤC / TRANG + NÚT TRÒN < >)
    // =========================================================================
    const pagerGroups = document.querySelectorAll('[data-aside-pager]');
    pagerGroups.forEach(function (group) {
        const pageSize = parseInt(group.getAttribute('data-page-size') || '5', 10);
        const items = Array.from(group.querySelectorAll('.aside-paged-item'));
        if (items.length <= pageSize) return; // Từ 5 mục trở xuống thì không cần hiện thanh lật trang

        const totalPages = Math.ceil(items.length / pageSize);
        let currentPage = 1;

        // Nếu đang chọn (active) một mục nằm ở trang 2, tự động mở sẵn trang đó
        const activeIndex = items.findIndex(function (el) { return el.classList.contains('active'); });
        if (activeIndex >= 0) {
            currentPage = Math.floor(activeIndex / pageSize) + 1;
        }

        // Tạo thanh điều hướng kiểu TopCV: "1/2" bên trái và 2 nút tròn "<" ">" bên phải
        const navBar = document.createElement('div');
        navBar.className = 'd-flex align-items-center justify-content-between pt-2 mt-2 border-top px-2';
        navBar.innerHTML = `
            <span class="aside-page-indicator text-muted fw-semibold" style="font-size: 13px;">1/${totalPages}</span>
            <div class="d-flex gap-2">
                <button type="button" class="aside-prev-btn d-flex align-items-center justify-content-center rounded-circle bg-white"
                        style="width: 28px; height: 28px; border: 1px solid #f7901e; color: #f7901e; transition: all 0.2s;">
                    <i class="bi bi-chevron-left" style="font-size: 12px;"></i>
                </button>
                <button type="button" class="aside-next-btn d-flex align-items-center justify-content-center rounded-circle bg-white"
                        style="width: 28px; height: 28px; border: 1px solid #f7901e; color: #f7901e; transition: all 0.2s;">
                    <i class="bi bi-chevron-right" style="font-size: 12px;"></i>
                </button>
            </div>
        `;
        group.appendChild(navBar);

        const indicator = navBar.querySelector('.aside-page-indicator');
        const prevBtn = navBar.querySelector('.aside-prev-btn');
        const nextBtn = navBar.querySelector('.aside-next-btn');

        function renderPage() {
            const start = (currentPage - 1) * pageSize;
            const end = start + pageSize;

            items.forEach(function (item, idx) {
                if (idx >= start && idx < end) {
                    item.style.setProperty('display', 'flex', 'important');
                } else {
                    item.style.setProperty('display', 'none', 'important');
                }
            });

            indicator.innerText = currentPage + '/' + totalPages;

            // Đổi màu nút khi ở đầu/cuối trang giống TopCV
            if (currentPage <= 1) {
                prevBtn.disabled = true;
                prevBtn.style.borderColor = '#e2e8f0';
                prevBtn.style.color = '#cbd5e1';
                prevBtn.style.cursor = 'not-allowed';
            } else {
                prevBtn.disabled = false;
                prevBtn.style.borderColor = '#f7901e';
                prevBtn.style.color = '#f7901e';
                prevBtn.style.cursor = 'pointer';
            }

            if (currentPage >= totalPages) {
                nextBtn.disabled = true;
                nextBtn.style.borderColor = '#e2e8f0';
                nextBtn.style.color = '#cbd5e1';
                nextBtn.style.cursor = 'not-allowed';
            } else {
                nextBtn.disabled = false;
                nextBtn.style.borderColor = '#f7901e';
                nextBtn.style.color = '#f7901e';
                nextBtn.style.cursor = 'pointer';
            }
        }

        prevBtn.addEventListener('click', function () {
            if (currentPage > 1) {
                currentPage--;
                renderPage();
            }
        });

        nextBtn.addEventListener('click', function () {
            if (currentPage < totalPages) {
                currentPage++;
                renderPage();
            }
        });

        renderPage();
    });


    // =========================================================================
    // 2. TỰ ĐỘNG ẨN THÔNG BÁO (ALERT) SAU 4 GIÂY
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
    // 3. HIỆU ỨNG SMOOTH SCROLL CHO SIDEBAR TRÊN MOBILE
    // =========================================================================
    if (window.innerWidth < 768) {
        const activeFilter = document.querySelector('.job-filter .aside-item.active');
        if (activeFilter) {
            activeFilter.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }
    }


    // =========================================================================
    // 4. HIỆU ỨNG CHUYỂN TRANG MƯỢT MÀ KHI CLICK BỘ LỌC HOẶC MENU
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

    window.addEventListener('pageshow', function () {
        document.body.style.opacity = '1';
    });


    // =========================================================================
    // 5. TẠO NÚT "QUAY LẠI ĐẦU TRANG" (BACK TO TOP) ĐỘNG
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
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });


    // =========================================================================
    // 6. HỘP THOẠI MODAL ĐẸP CHO: ĐĂNG XUẤT, ĐÓNG TIN, TỪ CHỐI, KHÓA TK, XEM THƯ
    // =========================================================================
    const logoutLinks = document.querySelectorAll('a[href*="Logout"]');
    logoutLinks.forEach(function (link) {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            document.body.style.opacity = '1';
            const targetUrl = this.href;
            showConfirmModal({
                title: "Đăng xuất tài khoản",
                message: "Bạn có chắc chắn muốn đăng xuất khỏi hệ thống D-JOBS không?",
                confirmText: "Đăng xuất",
                type: "warning",
                onConfirm: function () {
                    window.location.href = targetUrl;
                }
            });
        });
    });

    const closeJobBtns = document.querySelectorAll('.btn-close-job');
    closeJobBtns.forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const form = this.closest('form');
            showConfirmModal({
                title: "Đóng tin tuyển dụng",
                message: "Bạn có chắc chắn muốn tạm đóng tin tuyển dụng này không? (Bạn vẫn có thể mở lại bất cứ lúc nào).",
                confirmText: "Đóng tin",
                type: "danger",
                onConfirm: function () {
                    if (form) form.submit();
                }
            });
        });
    });

    const statusForms = document.querySelectorAll('form[action*="UpdateStatus"]');
    statusForms.forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (form.dataset.confirmed === "true") return;
            const statusInput = form.querySelector('input[name="newStatus"]');
            if (statusInput && statusInput.value === 'Rejected') {
                e.preventDefault();
                showConfirmModal({
                    title: "Từ chối hồ sơ ứng viên",
                    message: "Bạn có chắc chắn muốn TỪ CHỐI hồ sơ ứng viên này không? Thao tác này không thể hoàn tác.",
                    confirmText: "Từ chối hồ sơ",
                    type: "danger",
                    onConfirm: function () {
                        form.dataset.confirmed = "true";
                        form.submit();
                    }
                });
            }
        });
    });

    const toggleUserForms = document.querySelectorAll('form[action*="ToggleUserStatus"]');
    toggleUserForms.forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (form.dataset.confirmed === "true") return;
            e.preventDefault();
            const btn = form.querySelector('button[type="submit"]');
            const actionText = btn ? btn.innerText.trim() : 'Thay đổi';
            const isLock = actionText.toLowerCase().indexOf('khóa') !== -1 && actionText.toLowerCase().indexOf('mở') === -1;

            showConfirmModal({
                title: actionText + " tài khoản",
                message: "Bạn có chắc chắn muốn " + actionText.toLowerCase() + " tài khoản người dùng này không?",
                confirmText: actionText,
                type: isLock ? "danger" : "warning",
                onConfirm: function () {
                    form.dataset.confirmed = "true";
                    form.submit();
                }
            });
        });
    });

    const letterBtns = document.querySelectorAll('button[data-letter]');
    letterBtns.forEach(function (btn) {
        btn.removeAttribute('onclick');
        btn.addEventListener('click', function () {
            const letter = this.getAttribute('data-letter');
            showInfoModal("Thư giới thiệu ứng viên", letter, false);
        });
    });


    // =========================================================================
    // 7. KÍCH HOẠT BOOTSTRAP TOOLTIPS
    // =========================================================================
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }


    // =========================================================================
    // 8. HIỆU ỨNG ĐẾM SỐ LIỆU (COUNTER ANIMATION) CHO CÁC BADGE/COUNT
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
    // 9. KIỂM TRA FILE UPLOAD (CV PDF & LOGO CÔNG TY) BẰNG MODAL ĐẸP
    // =========================================================================
    const cvInput = document.querySelector('input[type="file"][name="cvFile"]');
    if (cvInput) {
        cvInput.addEventListener('change', function () {
            const file = this.files[0];
            if (!file) return;

            const fileName = file.name.toLowerCase();
            const maxSize = 5 * 1024 * 1024; // 5MB

            if (!fileName.endsWith('.pdf')) {
                showInfoModal("Định dạng không hợp lệ", "Hệ thống chỉ chấp nhận file CV định dạng PDF (.pdf)!", true);
                this.value = '';
                this.classList.add('is-invalid');
                return;
            }
            if (file.size > maxSize) {
                showInfoModal("File quá lớn", "Dung lượng file CV không được vượt quá 5MB!", true);
                this.value = '';
                this.classList.add('is-invalid');
                return;
            }
            this.classList.remove('is-invalid');
        });
    }

    const logoInput = document.querySelector('input[type="file"][name="logoFile"]');
    if (logoInput) {
        logoInput.addEventListener('change', function () {
            const file = this.files[0];
            if (!file) return;

            const validExts = ['.jpg', '.jpeg', '.png'];
            const fileName = file.name.toLowerCase();
            const isValid = validExts.some(function (ext) { return fileName.endsWith(ext); });

            if (!isValid) {
                showInfoModal("Định dạng ảnh không hợp lệ", "Vui lòng chọn file ảnh Logo định dạng .jpg, .jpeg hoặc .png!", true);
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
    // 10. TỰ ĐỘNG KIỂM TRA INPUT TRỐNG KHI SUBMIT FORM (CLIENT-SIDE VALIDATION)
    // =========================================================================
    const forms = document.querySelectorAll('form');
    forms.forEach(function (form) {
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