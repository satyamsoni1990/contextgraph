import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ContextExplorer } from './context-explorer';

describe('ContextExplorer', () => {
  let component: ContextExplorer;
  let fixture: ComponentFixture<ContextExplorer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContextExplorer],
    }).compileComponents();

    fixture = TestBed.createComponent(ContextExplorer);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
