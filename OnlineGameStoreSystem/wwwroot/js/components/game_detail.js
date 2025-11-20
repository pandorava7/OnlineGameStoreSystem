let currentSlide = 0;
const carouselInner = document.querySelector('.carousel-inner');
const slides = document.querySelectorAll('.carousel-item');

function showSlide(index) {
    if (index < 0) index = slides.length - 1;
    if (index >= slides.length) index = 0;
    currentSlide = index;
    carouselInner.style.transform = `translateX(-${currentSlide * 100}%)`;
}
function prevSlide() { showSlide(currentSlide - 1); }
function nextSlide() { showSlide(currentSlide + 1); }