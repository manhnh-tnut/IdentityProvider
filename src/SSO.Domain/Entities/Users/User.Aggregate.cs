using SSO.Domain.Base;
using SSO.Domain.Entities.Users.Events;

namespace SSO.Domain.Entities.Users
{
    public partial class User: IAggregateRoot
    {
        public User(string userName
            , string fullName
            , string address
            , DateTime? birthDate)
        {
            UserName = userName;

            this.Update(
                fullName
                , address
                , birthDate
            );
        }

        public void Update(string fullName
            , string address
            , DateTime? birthDate)
        {
            FullName = fullName;
            Address = address;
            BirthDate = birthDate;
        }

        public User Register(User user)
        {
            var addEvent = new OnRegisteredEvent()
            {
                User = user
            };
            AddEvent(addEvent);
            return user;
        }

        public void AddBranch(Guid branchId)
        {
            BranchId = branchId;
        }

        public void AddDepartment(Guid departmentId)
        {
            DepartmentId = departmentId;
        }

        public Avatar AddAvatar(string name)
        {
            // Make sure there's only one payslip  per month
            var avatar = Avatars.FirstOrDefault(_ => _.Name == name);
            if (avatar == null)
            {
                avatar = new Avatar(Id, name);
                Avatars.Add(avatar);
                var addEvent = new OnAvatarAddedEvent()
                {
                    Avatar = avatar
                };
                AddEvent(addEvent);
            }
            return avatar;
        }
        public Avatar ActiveAvatar(Guid avatarId)
        {
            foreach (var item in Avatars.Where(_ => _.IsActived))
            {
                item.IsActived = false;
            }
            var avatar = Avatars.FirstOrDefault(_ => _.Id == avatarId);
            if (avatar != null)
            {
                avatar.IsActived = true;
                var addEvent = new OnAvatarActivedEvent()
                {
                    Avatar = avatar
                };
                AddEvent(addEvent);
            }
            return avatar;
        }
    }
}