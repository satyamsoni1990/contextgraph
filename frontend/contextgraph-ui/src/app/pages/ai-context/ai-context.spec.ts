import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AiContext } from './ai-context';

describe('AiContext', () => {
  let component: AiContext;
  let fixture: ComponentFixture<AiContext>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AiContext],
    }).compileComponents();

    fixture = TestBed.createComponent(AiContext);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
