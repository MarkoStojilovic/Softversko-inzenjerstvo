import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListaOglasaComponent } from './lista-oglasa.component';

describe('ListaOglasaComponent', () => {
  let component: ListaOglasaComponent;
  let fixture: ComponentFixture<ListaOglasaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ListaOglasaComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ListaOglasaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
