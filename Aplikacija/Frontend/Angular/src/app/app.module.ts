import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule,ReactiveFormsModule } from '@angular/forms';


import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { IndexComponent } from './index/index.component';
import { ListaOglasaComponent } from './lista-oglasa/lista-oglasa.component';
import { ListaKorisnikaComponent } from './lista-korisnika/lista-korisnika.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgbModule } from  '@ng-bootstrap/ng-bootstrap';
import { CVComponent } from './cv/cv.component';

import { HttpClientModule, HTTP_INTERCEPTORS} from '@angular/common/http';
import { AngularEditorModule } from '@kolkov/angular-editor';
import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import { OglasiComponent } from './oglasi/oglasi.component';
import { UploadComponent } from './upload/upload.component';
import { ZahteviComponent } from './zahtevi/zahtevi.component';
import { InboxComponent } from './inbox/inbox.component';
import { HttpInterceptorService } from './interceptor/interceptor.service';


@NgModule({
  declarations: [
    AppComponent,
    IndexComponent,
    ListaOglasaComponent,
    ListaKorisnikaComponent,
    CVComponent,
    OglasiComponent,
    UploadComponent,
    ZahteviComponent,
    InboxComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    FontAwesomeModule,
    NgbModule,
   HttpClientModule, 
   AngularEditorModule ,
   ReactiveFormsModule,
  ],
  providers: [{

    provide: HTTP_INTERCEPTORS,

    useClass: HttpInterceptorService,

    multi: true,

  }],
  bootstrap: [AppComponent]
})
export class AppModule { }
