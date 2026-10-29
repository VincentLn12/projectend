import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { createPagedList } from '../../../../shared/data/paged-list';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { CoursesApi } from '../api/courses.api';
import { CourseListItem } from '../types/course.types';

@Component({
  selector: 'app-public-course-list',
  imports: [RouterLink, DataState, FileImage, Pagination, SearchBox],
  templateUrl: './course-list.html',
})
export class PublicCourseList implements OnInit {
  private readonly api = inject(CoursesApi);

  protected readonly list = createPagedList<CourseListItem>((query) => this.api.list(query), 9);

  ngOnInit(): void {
    this.list.load();
  }
}
