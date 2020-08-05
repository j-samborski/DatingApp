import { InjectableCompiler } from '@angular/compiler/src/injectable_compiler';
import { Injectable } from '@angular/core';

import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse,
  HTTP_INTERCEPTORS,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
        catchError(responseError => {
            if (responseError.status === 401){
                return throwError(responseError.statusText) //return error to component
            }
            if (responseError instanceof HttpErrorResponse){ //500 application error (from header)
                const applicationError = responseError.headers.get('Application-Error');
                if (applicationError){
                    return throwError(applicationError);
                }
            }
            const serverError = responseError.error; //modelstate errors (screenshot)
            let modelStateErrors = '';
            if (serverError.errors && typeof serverError.errors === 'object'){
                for (const key in serverError.errors) {
                    if (serverError.errors[key]) {
                        modelStateErrors += serverError.errors[key] + '\n';
                    }
                }
            }
            return throwError(modelStateErrors || serverError || 'Generic Server Error');
        })
    )
  }
}

export const ErrorInterceptorProvider = { //add this to the rest of existing interceptors
    provide: HTTP_INTERCEPTORS,
    useClass: ErrorInterceptor,
    multi: true
};