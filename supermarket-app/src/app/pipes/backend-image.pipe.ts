import { Pipe, PipeTransform } from '@angular/core';
import { environment } from '../../environments/environment';

@Pipe({
  name: 'backendImage'
})
export class BackendImagePipe implements PipeTransform {
  private backendUrl = environment.apiUrl;

  transform(relativeUrl: string | null | undefined): string {
    if (!relativeUrl) {
      return 'public/images/default-product.jpg'; // 如果後端沒給圖，自動回傳前端的預設圖
    }
    return `${this.backendUrl}${relativeUrl}`;;
  }

}
