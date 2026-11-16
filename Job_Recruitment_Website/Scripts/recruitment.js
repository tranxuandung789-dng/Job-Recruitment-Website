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

    // Hàm gọi Modal Xác nhận đẹp (Confirm)
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

    // Hàm gọi Modal Thông báo đẹp (Alert / Xem thư giới thiệu)
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
    // 1. TỰ ĐỘNG ẨN THÔNG BÁO (ALERT) SAU 4 GIÂY
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
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });


    // =========================================================================
    // 5. HỘP THOẠI MODAL ĐẸP CHO: ĐĂNG XUẤT, ĐÓNG TIN, TỪ CHỐI, KHÓA TK, XEM THƯ
    // =========================================================================
    // 5.1 Xác nhận Đăng xuất
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

    // 5.2 Xác nhận Đóng tin tuyển dụng (Jobs/MyJobs)
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

    // 5.3 Xác nhận Từ chối ứng viên (Applications/Candidates)
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

    // 5.4 Xác nhận Khóa / Mở khóa tài khoản (Admin/Index)
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

    // 5.5 Nút "Xem thư giới thiệu" (Cover Letter) hiển thị bằng Modal đẹp
    const letterBtns = document.querySelectorAll('button[data-letter]');
    letterBtns.forEach(function (btn) {
        btn.removeAttribute('onclick');
        btn.addEventListener('click', function () {
            const letter = this.getAttribute('data-letter');
            showInfoModal("Thư giới thiệu ứng viên", letter, false);
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
    // 8. KIỂM TRA FILE UPLOAD (CV PDF & LOGO CÔNG TY) BẰNG MODAL ĐẸP
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
    // 9. TỰ ĐỘNG KIỂM TRA INPUT TRỐNG KHI SUBMIT FORM (CLIENT-SIDE VALIDATION)
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