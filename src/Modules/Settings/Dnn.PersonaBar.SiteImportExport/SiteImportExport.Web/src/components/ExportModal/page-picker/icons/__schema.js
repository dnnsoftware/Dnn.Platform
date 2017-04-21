

import arrow_svg from './arrow_bullet.svg'

export class ArrowIcon extends Component {

    state ={
        selected:false
    }

    constructor(props){
        super()
        this.props = props;

    }

    ComponentDidMount(){
        this.update()
    }


    update(){
        let  {animate, reset} =  this.props
        this.animate(animate)
        this.reset(reset)
    }


    animate = (bool) =>{
        this.setState({selected:!this.state.selected})
    }

    reset = (bool) => {
        if(bool)
        this.setState({selected:false})
    }




    render(){
        return (
            <div>
                <img src={arrow_svg} />>
            </div>
        )
    }

}
