
export const ProvideAntiForgeryToken = () => {
   return  $('[name="__RequestVerificationToken"]').val()
}