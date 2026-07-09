document.addEventListener("DOMContentLoaded", function () {

    const revealElements = document.querySelectorAll(
        ".premium-text, .hero-trust-row, .device-card, .floating-card, .section-heading, .feature-card-pro, .category-card-pro, .workflow-step, .premium-cta, .custom-card, .stats-card"
    );

    revealElements.forEach((element, index) => {
        element.classList.add("reveal-item");
        element.style.transitionDelay = `${Math.min(index * 0.06, 0.35)}s`;
    });

    const revealObserver = new IntersectionObserver(
        function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("is-visible");
                }
            });
        },
        {
            threshold: 0.12
        }
    );

    revealElements.forEach(function (element) {
        revealObserver.observe(element);
    });


    // حركة كتابة الجزء الثاني فقط
    const animatedHeroText = document.getElementById("animatedHeroText");
    let typingTimer;

    function typeHeroText() {
        if (!animatedHeroText) return;

        clearTimeout(typingTimer);

        const lang = document.documentElement.lang;
        const text = lang === "en"
            ? animatedHeroText.getAttribute("data-en")
            : animatedHeroText.getAttribute("data-ar");

        animatedHeroText.textContent = "";

        let index = 0;

        function writeLetter() {
            if (index < text.length) {
                animatedHeroText.textContent += text.charAt(index);
                index++;
                typingTimer = setTimeout(writeLetter, 65);
            }
        }

        writeLetter();
    }

    typeHeroText();


    // زر اللغة
    const languageToggle = document.getElementById("languageToggle");

    if (languageToggle) {
        languageToggle.addEventListener("click", function () {
            const currentLang = document.documentElement.lang;
            const nextLang = currentLang === "ar" ? "en" : "ar";

            document.documentElement.lang = nextLang;
            document.documentElement.dir = nextLang === "ar" ? "rtl" : "ltr";

            languageToggle.textContent = nextLang === "ar" ? "EN" : "AR";

            document.querySelectorAll("[data-ar][data-en]").forEach(function (element) {
                if (element.id === "animatedHeroText") {
                    return;
                }

                element.textContent = nextLang === "ar"
                    ? element.getAttribute("data-ar")
                    : element.getAttribute("data-en");
            });

            typeHeroText();
        });
    }
});