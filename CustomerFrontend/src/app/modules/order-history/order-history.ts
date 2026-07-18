import { Component, OnInit, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerService } from '../customer.service';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../auth.service';

interface OrderItem {
  orderId: number;
  itemName: string;
  expiryDate: Date;
  quantity: number;
  price: number;
  status: string;
}

interface GroupedOrder {
  orderGroupId: string;
  orderDate: Date;
  items: OrderItem[];
  totalAmount: number;
  itemCount: number;
}

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-history.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './order-history.css',
})
export class OrderHistory implements OnInit {
  orderHistory = signal<GroupedOrder[]>([]);
  isLoading = signal(false);
  errorMessage = signal('');
  expandedOrders = signal<Set<string>>(new Set());

  constructor(private customerService: CustomerService, private router: Router, private authService: AuthService, private route: ActivatedRoute) {}

  ngOnInit() {
    if (!this.authService.getIsAuthenticated()) {
      return;
    }
    // Subscribe to route changes to reload data when navigating to this page
    this.route.params.subscribe(() => {
      this.loadOrderHistory();
    });
  }

  loadOrderHistory() {
    this.isLoading.set(true);
    this.customerService.getOrderHistory().subscribe({
      next: (response) => {
        console.log('Order history response:', response);
        this.orderHistory.set(response.data || []);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error fetching order history from backend:', error);
        this.errorMessage.set('Failed to load order history. Please ensure backend is running.');
        this.isLoading.set(false);
      },
    });
  }

  isExpiringSoon(expiryDate: Date): boolean {
    const today = new Date();
    const expiry = new Date(expiryDate);
    const diffTime = expiry.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays <= 7 && diffDays > 0;
  }

  isExpired(expiryDate: Date): boolean {
    const today = new Date();
    const expiry = new Date(expiryDate);
    return expiry < today;
  }

  toggleOrderExpansion(orderGroupId: string) {
    const currentExpanded = this.expandedOrders();
    const newExpanded = new Set(currentExpanded);
    if (newExpanded.has(orderGroupId)) {
      newExpanded.delete(orderGroupId);
    } else {
      newExpanded.add(orderGroupId);
    }
    this.expandedOrders.set(newExpanded);
  }

  isOrderExpanded(orderGroupId: string): boolean {
    return this.expandedOrders().has(orderGroupId);
  }

  getOrderExpiryStatus(order: GroupedOrder): { status: string; class: string } {
    // Check if any item is expired
    const hasExpired = order.items.some(item => this.isExpired(item.expiryDate));
    // Check if any item is expiring soon
    const hasExpiringSoon = order.items.some(item => this.isExpiringSoon(item.expiryDate));
    
    if (hasExpired) {
      return { status: 'Expired', class: 'expired' };
    } else if (hasExpiringSoon) {
      return { status: 'Expiring Soon', class: 'expiring-soon' };
    } else {
      return { status: 'Active', class: 'active' };
    }
  }

  navigateToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  logout() {
    localStorage.removeItem('Mytoken');
    this.authService.setAuthenticated(false);
    this.router.navigate(['/home']);
  }

  getIsAuthenticated() {
    return this.authService.getIsAuthenticated();
  }

  navigateToLogin() {
    this.router.navigate(['/login']);
  }
}
