import { Entity } from './entity';

export class Document extends Entity {
  children: Document[];
  index: number;
}

export class Project extends Document {

}

export class Workspace {
  project: Project;
  texts: {};
}