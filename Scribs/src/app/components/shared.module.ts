import { CommonModule } from '@angular/common';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';

import { ToolbarComponent } from './toolbar/toolbar.component';
import { SettingsComponent } from './settings/settings.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { ActionComponent } from './sidebar/action/action.component';
import { ExplorerComponent } from './sidebar/explorer/explorer.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule
  ],
  declarations: [
    ActionComponent,
    ExplorerComponent,
    ToolbarComponent,
    SettingsComponent,
    SidebarComponent,
  ],
  exports: [
    ActionComponent,
    ExplorerComponent,
    ToolbarComponent,
    SettingsComponent,
    SidebarComponent
  ],
  entryComponents: [SettingsComponent],
})
export class SharedModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: SharedModule,
      providers: [
        // ToastComponent
      ]
    };
  }
}
