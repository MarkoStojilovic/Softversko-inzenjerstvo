import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CookieService } from "ngx-cookie-service";
import { Observable } from "rxjs/internal/Observable";

@Injectable({

  providedIn: 'root'

})

export class HttpInterceptorService implements HttpInterceptor {

  constructor( private cookie:CookieService)
{
  }
   intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    const token = this.cookie.get("Token");
    if (token) {

      request = request.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      })
    }



    return next.handle(request);

  }

}