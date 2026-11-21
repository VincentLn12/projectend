import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { createPagedList } from '../../../../shared/data/paged-list';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { ShortCoursesApi } from '../api/short-courses.api';
import { AVAILABILITY_BADGE_CLASSES, AVAILABILITY_LABELS, ShortCourseListItem } from '../types/short-course.types';

const PAGE_SIZE = 9;

@Component({
  selector: 'app-public-short-course-list',
  imports: [RouterLink, ThaiDatePipe, Button, DataState, FileImage, Pagination, SearchBox],
  templateUrl: './short-course-list.html',
})
export class PublicShortCourseList implements OnInit {
  private readonly api = inject(ShortCoursesApi);

  protected readonly list = createPagedList<ShortCourseListItem>((query) => this.api.list(query), PAGE_SIZE);

  protected readonly availabilityLabels = AVAILABILITY_LABELS;
  protected readonly availabilityBadgeClasses = AVAILABILITY_BADGE_CLASSES;

  ngOnInit(): void {
    this.list.load();
  }

  protected seatsText(item: ShortCourseListItem): string {
    if (item.maxParticipants === 0) {
      return `สมัครแล้ว ${item.registeredCount} คน`;
    }
    const left = Math.max(item.maxParticipants - item.registeredCount, 0);
    return `เหลือ ${left} จาก ${item.maxParticipants} ที่นั่ง`;
  }
}
