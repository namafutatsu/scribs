import { Entity } from './entity';

export class Document extends Entity {
  children: Document[];
  index: number;
}

export class Project extends Document {

}

export class Text extends Entity {
  content: string;
}

export class Workspace {
  project: Project;
  texts: {};
}