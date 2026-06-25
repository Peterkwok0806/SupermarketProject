import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

interface BannerSlide {
  image: string;
  alt: string;
}

@Component({
  selector: 'app-banner',
  imports: [CommonModule],
  templateUrl: './banner.component.html',
  styleUrl: './banner.component.css'
})
export class BannerComponent implements OnInit, OnDestroy {
  slides: BannerSlide[] = [
    { image: '/images/banner/HB-Food-FrozenMeat-20260625-C.avif', alt: 'Frozen Meat Promotion' },
    { image: '/images/banner/HB-Food-BeKind-20260709-C.jpg', alt: 'Be Kind Food' },
    { image: '/images/banner/HB-Food-Knife-20260625-C.avif', alt: 'Kitchen Knife' },
    { image: '/images/banner/HB-Food-TungChun-20260625-C.avif', alt: 'Tung Chun' },
  ];

  currentSlide = signal(0);
  isTransitioning = signal(false);
  private autoPlayInterval: ReturnType<typeof setInterval> | null = null;

  ngOnInit() {
    this.startAutoPlay();
  }

  ngOnDestroy() {
    this.stopAutoPlay();
  }

  startAutoPlay() {
    this.autoPlayInterval = setInterval(() => {
      this.nextSlide();
    }, 4000);
  }

  stopAutoPlay() {
    if (this.autoPlayInterval) {
      clearInterval(this.autoPlayInterval);
      this.autoPlayInterval = null;
    }
  }

  nextSlide() {
    this.isTransitioning.set(true);
    setTimeout(() => {
      this.currentSlide.update(i => (i + 1) % this.slides.length);
      setTimeout(() => this.isTransitioning.set(false), 50);
    }, 300);
  }

  prevSlide() {
    this.isTransitioning.set(true);
    setTimeout(() => {
      this.currentSlide.update(i => (i - 1 + this.slides.length) % this.slides.length);
      setTimeout(() => this.isTransitioning.set(false), 50);
    }, 300);
  }

  goToSlide(index: number) {
    this.stopAutoPlay();
    this.isTransitioning.set(true);
    setTimeout(() => {
      this.currentSlide.set(index);
      setTimeout(() => this.isTransitioning.set(false), 50);
    }, 300);
    this.startAutoPlay();
  }

  onMouseEnter() {
    this.stopAutoPlay();
  }

  onMouseLeave() {
    this.startAutoPlay();
  }
}