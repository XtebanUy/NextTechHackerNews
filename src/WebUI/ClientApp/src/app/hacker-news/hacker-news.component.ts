import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import {MatTableModule} from '@angular/material/table';
import { PaginatedListOfStoryDto, StoryClient, StoryDto } from '../web-api-client';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DataSource } from '@angular/cdk/table';
import { CollectionViewer } from '@angular/cdk/collections';
import { BehaviorSubject, Observable, Subject, Subscription, catchError, map, of, switchMap } from 'rxjs';
import { PageEvent } from '@angular/material/paginator';




@Component({
  selector: 'app-hacker-news',
  templateUrl: './hacker-news.component.html',
  styleUrls: ['./hacker-news.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HackerNewsComponent implements OnInit {
  displayedColumns: string[] = ['id', 'news', 'score', 'time', 'by'];
  dataSource : StoryDataSource;
  pageInformation: Observable<PageInformation>;
  form: FormGroup;
  pageSizeOptions: number[] = [10, 20, 50];
  pageSize: number = 10;
  searchValue?: {title: string | null | undefined, minScore: number | null | undefined, maxScore: number | null | undefined, minTime: Date | null | undefined, maxTime: Date | null | undefined};
  constructor(private storyClient: StoryClient, private fb: FormBuilder)
  {
    this.dataSource = new StoryDataSource(this.storyClient);
    this.pageInformation = this.dataSource.pageInformation;
    this.form = this.fb.group({
        title: [''],
        minScore: ['', [Validators.min(0), Validators.pattern("^[0-9]*$")]],
        maxScore: ['', [Validators.min(0), Validators.pattern("^[0-9]*$")]] ,
        minDate: [''],
        maxDate: ['']
    });
  }
  ngOnInit(): void {

  }

  onSubmit(): void {
    if (this.form.valid)
    {
        this.searchValue = this.form.value;
        this.dataSource.load(this.searchValue!, 1, this.pageSize);
    }
  }

  handlePageEvent(e: PageEvent) {
    let pageIndex = 1
    
    if (e.pageSize == this.pageSize)
    {
      pageIndex = e.pageIndex + 1;
    }

    this.pageSize = e.pageSize;
    this.dataSource.load(this.searchValue!, pageIndex, e.pageSize);
  }
}


export class StoryDataSource implements DataSource<StoryDto>
{
  private stories: BehaviorSubject<readonly StoryDto[]> = new BehaviorSubject<readonly StoryDto[]>([]);
  private _pageInformation: BehaviorSubject<PageInformation> = new BehaviorSubject({pageNumber: 1, pageSize: 10, totalPages: 0, totalCount: 0, hasPreviousPage: false, hasNextPage: false} as PageInformation);
  private _requestSubject: Subject<{filter : {title: string | null | undefined, minScore: number | null | undefined, maxScore: number | null | undefined, minTime: Date | null | undefined, maxTime: Date | null | undefined}, pageNumber: number | undefined, pageSize: number | undefined}> = new Subject()
  private _requestSubjectSubscription?: Subscription;
  constructor(private storyClient: StoryClient)
  {
     this._requestSubjectSubscription = this._requestSubject.pipe(
      switchMap(({filter, pageNumber, pageSize}) => this.storyClient.get(filter.title, filter.minScore, filter.maxScore, filter.minTime, filter.maxTime, pageNumber, pageSize)
      .pipe(
        map((result) => ({result, pageSize})))),
        catchError((err, obs) => of(
          {
            result: new PaginatedListOfStoryDto(
              {
                items: [],
                hasNextPage: false,
                hasPreviousPage: false,
                pageNumber: 1,
                totalCount: 0,
                totalPages: 0
              }),
            pageSize: 10
          })))
        .subscribe(c => {
            const {items, ...pageInformation} = c.result;
            this.stories.next(items ?? []);
          this._pageInformation.next({...pageInformation, pageSize: c.pageSize});
        })
  }

  connect(collectionViewer: CollectionViewer): Observable<readonly StoryDto[]> {
    return this.stories.asObservable();
  }

  disconnect(collectionViewer: CollectionViewer): void {
    this.stories.complete();
    this._requestSubjectSubscription?.unsubscribe();
  }

  load(filter : {title: string | null | undefined, minScore: number | null | undefined, maxScore: number | null | undefined, minTime: Date | null | undefined, maxTime: Date | null | undefined}, pageNumber: number | undefined, pageSize: number | undefined)
  {
    this._requestSubject.next({filter, pageNumber, pageSize});
  }

  public get pageInformation() :Observable<PageInformation>
  {
    return this._pageInformation.asObservable();
  }
  
}

export interface PageInformation { pageNumber?: number, pageSize?: number, totalPages?: number, totalCount?: number, hasPreviousPage?: boolean, hasNextPage?: boolean }
