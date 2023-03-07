import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {IndexComponent} from "../app/index/index.component"
import { ListaKorisnikaComponent } from './lista-korisnika/lista-korisnika.component';
import { ListaOglasaComponent } from './lista-oglasa/lista-oglasa.component';
import { CVComponent } from './cv/cv.component';
import { OglasiComponent } from './oglasi/oglasi.component';
import { ZahteviComponent } from './zahtevi/zahtevi.component';
import { InboxComponent } from './inbox/inbox.component';


const routes: Routes = [
  {path:"Index", component:IndexComponent},
  {path:"Lista korisnika", component:ListaKorisnikaComponent},
  {path:"Lista oglasa", component:ListaOglasaComponent},
  {path:"CV" , component:CVComponent},
  {path:"Oglas", component:OglasiComponent },
  {path:"Zahtevi",component:ZahteviComponent},
  {path:"Inbox",component:InboxComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule { }
