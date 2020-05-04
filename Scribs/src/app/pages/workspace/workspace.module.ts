import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { IonicModule } from '@ionic/angular';

import { WorkspacePageRoutingModule } from './workspace-routing.module';

import { SharedModule } from 'src/app/components/shared.module';
import { WorkspacePage } from './workspace.page';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    SharedModule,
    WorkspacePageRoutingModule
  ],
  declarations: [WorkspacePage]
})
export class WorkspacePageModule {}
