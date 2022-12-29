
addEventListener('DOMContentLoaded', (event) => {
    console.log(document.getElementById('main-block'))
    setTimeout(() => {
        document.getElementById('main-block')?.scrollIntoView({
            block: 'center', inline: 'center'
        });
    },2000)

});
  
