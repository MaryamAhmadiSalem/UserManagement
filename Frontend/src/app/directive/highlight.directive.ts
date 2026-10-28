import { Directive, ElementRef, HostListener, Renderer2 } from '@angular/core';
import { inject } from '@angular/core';

@Directive({
  selector: '[appHighlight]',
})
export class Highlight {
  // to change style
  private renderer = inject(Renderer2);
  // to access the host element
  private el = inject(ElementRef);
  private orginalColor = " "

  ngOnInit(){
    // get the elemnt style whit getComputedStyle
    this.orginalColor = getComputedStyle(this.el.nativeElement).backgroundColor
  }
  // to receive the event
  @HostListener('mouseenter')
  onmouseenter(){
    this.renderer.setStyle(this.el.nativeElement, 'background-color', 'beige');
  }

  @HostListener('mouseleave')
  onMouseLeave(){
    this.renderer.setStyle(this.el.nativeElement, 'background-color', this.orginalColor)
  }
}