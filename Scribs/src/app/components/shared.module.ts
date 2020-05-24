import { CommonModule } from '@angular/common';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
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
import { NamingComponent } from './naming/naming.component';

@NgModule({
  imports: [
    CommonModule,
    IonicModule,
    FormsModule,
    FroalaEditorModule,
    ReactiveFormsModule,
    TreeModule
  ],
  declarations: [
    ActionComponent,
    EditorComponent,
    ExplorerComponent,
    MenuComponent,
    NamingComponent,
    SettingsComponent,
    SidebarComponent,
    ToolbarComponent
  ],
  exports: [
    ActionComponent,
    EditorComponent,
    ExplorerComponent,
    MenuComponent,
    NamingComponent,
    SettingsComponent,
    SidebarComponent,
    ToolbarComponent
  ],
  entryComponents: [
    NamingComponent,
    SettingsComponent
  ],
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
