import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { NamingComponent } from './naming.component';

describe('NamingComponent', () => {
  let component: NamingComponent;
  let fixture: ComponentFixture<NamingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ NamingComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(NamingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
