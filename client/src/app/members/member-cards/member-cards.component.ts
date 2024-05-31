import { Component, Input, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-cards',
  templateUrl: './member-cards.component.html',
  styleUrls: ['./member-cards.component.css'],
})
export class MemberCardsComponent implements OnInit {
  @Input() member: Member | undefined;

  constructor(
    private memberServices: MembersService,
    private toastr: ToastrService
  ) {}
  ngOnInit(): void {}

  addLike(member: Member) {
    this.memberServices.addLike(member.userName).subscribe({
      next: () => this.toastr.success('You have liked' + member.knownAs),
    });
  }
}
