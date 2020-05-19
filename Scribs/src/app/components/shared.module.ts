import { CommonModule } from '@angular/common';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';

import { FroalaEditorModule } from 'angular-froala-wysiwyg';
import { TreeModule } from 'angular-tree-component';

import { ToolbarComponent } from './toolbar/toolbar.component';
import { SettingsComponent } from './settings/settings.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { ActionComponent } from './sidebar/action/action.component';
import { ExplorerComponent } from './sidebar/explorer/explorer.component';
import { EditorComponent } from './editor/editor.component';
import { MenuComponent } from './menu/menu.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    FroalaEditorModule,
    TreeModule,
    IonicModule
  ],
  declarations: [
    ActionComponent,
    EditorComponent,
    ExplorerComponent,
    MenuComponent,
    ToolbarComponent,
    SettingsComponent,
    SidebarComponent,
  ],
  exports: [
    ActionComponent,
    EditorComponent,
    ExplorerComponent,
    MenuComponent,
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
