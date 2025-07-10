import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { VendorModel } from './vendor.model';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root', // Makes the service a singleton throughout the application
})
export class VendorService {
  private apiUrl = environment.apiUrl + '/api/Vendors';

  private vendorsCache: VendorModel[] | null = null;
  private cacheExpirationTime: number | null = null;
  private readonly CACHE_DURATION_MS = 5 * 60 * 1000; // 5 minutes in milliseconds

  constructor(private http: HttpClient) {}

  /**
   * Fetches VendorModel[] from the API or returns from cache.
   *
   * @param forceRefresh - If true, bypasses the cache and fetches from the API.
   * @returns An Observable of VendorModel[].
   */
  getVendors(forceRefresh: boolean = false): Observable<VendorModel[]> {
    // Check if a refresh is forced or if the cache is empty/expired
    if (forceRefresh || !this.vendorsCache || this.isCacheExpired()) {
      console.log('Fetching vendors from API...');
      return this.http.get<VendorModel[]>(this.apiUrl).pipe(
        tap(data => {
          // Cache the data upon successful API call
          this.vendorsCache = data;
          this.cacheExpirationTime = Date.now() + this.CACHE_DURATION_MS;
          console.log('Vendors cached:');
        }),
        catchError(error => {
          console.error('Error fetching vendors from API:', error);
          // Optionally clear cache on error if data might be stale
          this.vendorsCache = null;
          this.cacheExpirationTime = null;
          return throwError(() => new Error('Failed to load vendors.'));
        })
      );
    } else {
      console.log('Returning vendors from cache...');
      // Return the cached data as an Observable
      return of(this.vendorsCache);
    }
  }

  clearVendorsCache(): void {
    this.vendorsCache = null;
    this.cacheExpirationTime = null;
    console.log('Vendor cache cleared.');
  }

  private isCacheExpired(): boolean {
    if (!this.cacheExpirationTime) {
      return true; // No expiration time set, so consider it expired
    }
    return Date.now() > this.cacheExpirationTime;
  }
}
