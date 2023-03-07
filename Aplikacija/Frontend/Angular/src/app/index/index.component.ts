import { Component, OnInit } from '@angular/core';
import { faBriefcase  } from '@fortawesome/free-solid-svg-icons';
import { faChalkboardUser} from '@fortawesome/free-solid-svg-icons';
import { faEnvelope} from '@fortawesome/free-solid-svg-icons';
import { faSquare} from '@fortawesome/free-solid-svg-icons';
import { faInstagram } from '@fortawesome/free-brands-svg-icons';
import {NgbModal} from '@ng-bootstrap/ng-bootstrap';
import { AppComponent } from '../app.component';


@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})

export class IndexComponent implements OnInit {
//Ikonice koje se koriste za UI//
  faBriefcase = faBriefcase;
  faChalkboardUser = faChalkboardUser;
  faEnvelope = faEnvelope;
  faSquare = faSquare;
  faInstagram = faInstagram;
//----------------------------//
  constructor(private modalService: NgbModal, private app:AppComponent) {}
  ngOnInit(): void {}
}
