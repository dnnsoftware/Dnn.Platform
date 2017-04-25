
import React, {Component} from 'react';
const arrow_svg = require("!raw!./arrow_bullet.svg");
import {global} from '../../_global';

const styles = global.styles;
const greenscreen = styles.backgroundColor('green');
const floatLeft = styles.float();
const merge = styles.merge;

const inlineBlock = styles.display('inline-block');


import * as shortid from 'shortid';

const style = (direction) => {
  return {
    transition: `all .15s ease-in`,
    cursor:'pointer',
    width:'100%',
    transform: `rotate(${direction})`
  };
}

export class ArrowIcon extends Component {

    state ={
        selected:false,
        arrow_bullet: null,
    }

    constructor(props){
        super();
        this.props = props;
        this.direction = props.direction;
        this.id =  shortid.generate();
        this.shouldAnimate = props.animate;
    }


    onMouseDown = (e) => {
        if(!this.state.arrow_bullet){
            this.arrow_bullet = document.getElementById(this.id);
        }
        this.animate(this.shouldAnimate);
    }

    animate = (bool) =>{
        const left = () => {
          this.setState({selected:!this.state.selected});
          const arrow_css = this.arrow_bullet.style;
          arrow_css.transform = (this.state.selected) ? "rotate(0deg)" : "rotate(90deg)";
        }
        const right = () => null;
        bool ? left() : right();
    }

    reset = (bool) => {
        if(bool){
            this.setState({selected:false});
        }
    }


    render(){
      /* eslint-disable react/no-danger */
      const marginTop = styles.margin({top:-3});
      const padding = styles.padding({all:2});
      const baseStyles = style(this.direction);
        return (
            <div
                dangerouslySetInnerHTML={{ __html: arrow_svg }}
                id={this.id}
                style={merge(marginTop, padding, baseStyles)}
                ref={this.id}
                src={arrow_svg}
                alt="arrow_icon"
                onMouseDown={this.onMouseDown} />
        )
        /* eslint-disable react/no-danger */
    }

}
