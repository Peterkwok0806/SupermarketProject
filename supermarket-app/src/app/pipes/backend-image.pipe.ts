import { Pipe, PipeTransform } from '@angular/core';
import { environment } from '../../environments/environment';

@Pipe({
  name: 'backendImage'
})
export class BackendImagePipe implements PipeTransform {
  private backendUrl = environment.apiUrl;

    transform(relativeUrl: string | null | undefined): string {
      if (!relativeUrl) {
        return `${this.backendUrl}images/products/default-product.jpg`;
      }
      return `${this.backendUrl}images/products/${relativeUrl}`;
    }

}
