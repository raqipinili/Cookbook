import { HttpInterceptor, HttpEvent, HttpHandler, HttpRequest, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

export class HttpErrorInterceptor implements HttpInterceptor {
    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(request).pipe(
            catchError((error: HttpErrorResponse) => {
                let errMsg = '';

                // Client Side Error
                if (error.error instanceof ErrorEvent) {
                    errMsg = `Error: ${error.error.message}`;

                } else {
                    // Server Side Error

                    if (error.status === 401) {
                        errMsg = 'Access denied';
                    } else {
                        // errMsg = `Error Code: ${error.status},  Message: ${error.message}`;
                        return throwError(error);
                    }
                }

                return throwError(errMsg);
            })
        );
    }
}
