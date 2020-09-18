
dnn.extend(dnn.dom,{tweens:[],animate:function(ctl,type,dir,easingType,easingDir,length,interval,onStartFunc,onFinishFunc)
{var dims=new dnn.dom.positioning.dims(ctl);var anims=[];var vertical=(dir==dnn.motion.animationDir.Down||dir==dnn.motion.animationDir.Up);var expand=(dir==dnn.motion.animationDir.Down||dir==dnn.motion.animationDir.Right);var coord=vertical?'t':'l';var size=coord=='t'?'h':'w';var prop=coord=='t'?'top':'left';var easingFunc=dnn.getEnumByValue(dnn.motion.easingDir,easingDir)+dnn.getEnumByValue(dnn.motion.easingType,easingType);if(type==dnn.motion.animationType.Slide)
{anims.push({prop:'clip'+prop,begin:dims[size],finish:0});anims.push({prop:prop,begin:dims[coord]-dims[size],finish:dims[coord]});}
else if(type==dnn.motion.animationType.Expand)
{var newDir=vertical?'bottom':'right';anims.push({prop:'clip'+newDir,begin:0,finish:dims[size]});}
else if(type==dnn.motion.animationType.Diagonal)
{size=dims.h>dims.w?'h':'w';anims.push({prop:'clipbottomright',begin:0,finish:dims[size]});}
else if(type==dnn.motion.animationType.ReverseDiagonal)
{size=dims.h>dims.w?'h':'w';anims.push({prop:'cliptopleft',begin:dims[size],finish:0});}
for(var i=0;i<anims.length;i++)
{if(expand)
dnn.dom.doTween(ctl.id+i,ctl,anims[i].prop,dnn.easing[easingFunc],anims[i].begin,anims[i].finish,length,interval,'px',onStartFunc,onFinishFunc);else
dnn.dom.doTween(ctl.id+i,ctl,anims[i].prop,dnn.easing[easingFunc],anims[i].finish,anims[i].begin,length,interval,'px',onStartFunc,onFinishFunc);}},doTween:function(id,ctl,prop,func,begin,finish,duration,interval,suffix,onStartFunc,onFinishFunc)
{var tween=this.tweens[id];if(tween)
{tween.stop();tween.dispose();tween=null;}
tween=new dnn.tween(id,ctl.style,prop,func,begin,finish,duration);this.tweens[id]=tween;if(suffix)
tween.set_Suffix(suffix);if(interval)
tween.set_Interval(interval);tween.addEventHandler('onMotionStarted',dnn.dom.onMotionStarted);if(onStartFunc)
tween.addEventHandler('onMotionStarted',onStartFunc);tween.addEventHandler('onMotionFinished',dnn.dom.onMotionFinished);if(onFinishFunc)
tween.addEventHandler('onMotionFinished',onFinishFunc);tween.start();return tween;},onMotionFinished:function(arg)
{dnn.dom.tweens[arg.id]=null;arg.dispose();},onMotionStarted:function(arg)
{}});Type.registerNamespace('dnn.motion');dnn.motion=function(id,obj,prop,begin,duration)
{this.id=id;this._obj=obj;this._prop=prop;this._begin=begin;this._duration=duration;this._interval=10;this._finish=0;this._pos=0;this._prevPos=0;this._time=0;this._position=0;this._startTime=0;this._looping=false;this._suffix='px';this._listeners=[];this._events=new Sys.EventHandlerList();}
dnn.motion.prototype={get_Position:function()
{},set_Position:function(val)
{this._prevPos=val;var newVal=Math.round(val)+this._suffix;if(this._prop.indexOf('clip')==0)
{var clip=this._getClipObject();var clips=this._getClips(this._prop);for(var i=0;i<clips.length;i++)
clip[clips[i]]=newVal;this._obj.clip='rect('+clip.cliptop+','+clip.clipright+','+clip.clipbottom+','+clip.clipleft+')';}
else
this._obj[this._prop]=newVal;this._pos=val;this.raiseEvent('onMotionChanged');},get_PrevPosition:function()
{return this._prevPos;},set_Time:function(val)
{if(val>this._duration)
{if(this._looping)
{this.rewind(val-this._duration);this._update();this.raiseEvent('onMotionLooped');}
else
{this._time=this._duration;this._update();this.stop();this.raiseEvent('onMotionFinished');}}
else if(val<0)
{this.rewind();this._update();}
else
{this._time=val;this._update();}},get_Time:function(){return this._time;},set_Begin:function(val){this._begin=val;},get_Begin:function(){return this._begin;},set_Duration:function(val){this._duration=val;},get_Duration:function(){return this._duration;},set_Interval:function(val){this._interval=val;},get_Interval:function(){return this._interval;},set_Looping:function(val){this._looping=val;},get_Looping:function(){return this._looping;},set_Object:function(val){this._obj=val;},get_Object:function(){return this._obj;},set_Property:function(val){this._prop=val;},get_Property:function(){return this._prop;},set_Suffix:function(val){this._suffix=val;},get_Suffix:function(){return this._suffix;},get_Timer:function()
{return new Date().getTime()-this._time;},start:function()
{this._fixTime();this.isPlaying=true;this._onEnterFrame();this.raiseEvent('onMotionStarted');},stop:function()
{dnn.cancelDelay(this.id);this.isPlaying=false;this.raiseEvent('onMotionStopped');},resume:function()
{this._fixTime();this.start();},rewind:function(time)
{this._time=(time==null?1:time);this._fixTime();},fforward:function()
{this.set_Time(this._duration);this._fixTime();},nextFrame:function()
{this.set_Time((this.get_Timer()-this._startTime)/1000);},prevFrame:function()
{this.set_Time(this._time-1);},dispose:function()
{this._obj=null;this._listeners=null;this._events=null;},toString:function()
{return'[Motion prop='+this._prop+' t='+this._time+' pos='+this._pos+']';},addEventHandler:function(name,handler)
{this._events.addHandler(name,handler);},removeEventHandler:function(name,handler)
{this._events.removeHandler(name,handler);},raiseEvent:function(eventName)
{if(!this._events)return;var handler=this._events.getHandler(eventName);if(handler){handler(this,Sys.EventArgs.Empty);}},_onEnterFrame:function()
{if(this.isPlaying)
{this.nextFrame();dnn.doDelay(this.id,this._interval,dnn.createDelegate(this,this._onEnterFrame));}},_update:function()
{this.set_Position(this.get_Position(this._time));},_fixTime:function()
{this._startTime=this.get_Timer()-this._time;},_getClipObject:function()
{var o={};o.cliptop='auto';o.clipright='auto';o.clipbottom='auto';o.clipleft='auto';return o;},_getClips:function(prop)
{var o=[];if(prop.indexOf('top')>-1)
o.push('cliptop');if(prop.indexOf('left')>-1)
o.push('clipleft');if(prop.indexOf('bottom')>-1)
o.push('clipbottom');if(prop.indexOf('right')>-1)
o.push('clipright');return o;}}
dnn.motion.registerClass('dnn.motion',null,Sys.IDisposable);dnn.motion.animationType=function(){};dnn.motion.animationType.prototype={None:0,Slide:1,Expand:2,Diagonal:3,ReverseDiagonal:4}
dnn.motion.animationType.registerEnum("dnn.motion.animationType");dnn.motion.animationDir=function(){};dnn.motion.animationDir.prototype={Up:0,Down:1,Left:2,Right:3}
dnn.motion.animationDir.registerEnum("dnn.motion.animationDir");dnn.motion.easingType=function(){};dnn.motion.easingType.prototype={Bounce:0,Circ:1,Cubic:2,Expo:3,Quad:4,Quint:5,Quart:6,Sine:7}
dnn.motion.easingType.registerEnum("dnn.motion.easingType");dnn.motion.easingDir=function(){};dnn.motion.easingDir.prototype={easeIn:0,easeOut:1,easeInOut:2}
dnn.motion.easingDir.registerEnum("dnn.motion.easingDir");dnn.tween=function(id,obj,prop,func,begin,finish,duration)
{dnn.tween.initializeBase(this,[id,obj,prop,begin,duration]);this._func=func;this._change=0;this.set_Finish(finish);}
dnn.tween.prototype={set_Function:function(val){this._func=val;},get_Function:function(){return this._func;},set_Change:function(val){this._change=val;},get_Change:function(){return this._change;},set_Finish:function(val)
{this._change=val-this._begin;},get_Finish:function()
{return this._begin+this._change;},get_Position:function()
{return this._func(this._time,this._begin,this._change,this._duration);},continueTo:function(finish,duration)
{this.set_Begin(this.get_Position());this.set_Finish(finish);this.set_Duration(duration);this.start();},dispose:function()
{dnn.tween.callBaseMethod(this,'dispose');this._func=null;}}
dnn.tween.registerClass('dnn.tween',dnn.motion);Type.registerNamespace('dnn.easing');dnn.extend(dnn.easing,{linearTween:function(t,b,c,d){return c*t/d+b;},easeInQuad:function(t,b,c,d){return c*(t/=d)*t+b;},easeOutQuad:function(t,b,c,d){return-c*(t/=d)*(t-2)+b;},easeInOutQuad:function(t,b,c,d){if((t/=d/2)<1)return c/2*t*t+b;return-c/2*((--t)*(t-2)-1)+b;},easeInCubic:function(t,b,c,d){return c*(t/=d)*t*t+b;},easeOutCubic:function(t,b,c,d){return c*((t=t/d-1)*t*t+1)+b;},easeInOutCubic:function(t,b,c,d){if((t/=d/2)<1)return c/2*t*t*t+b;return c/2*((t-=2)*t*t+2)+b;},easeInQuart:function(t,b,c,d){return c*(t/=d)*t*t*t+b;},easeOutQuart:function(t,b,c,d){return-c*((t=t/d-1)*t*t*t-1)+b;},easeInOutQuart:function(t,b,c,d){if((t/=d/2)<1)return c/2*t*t*t*t+b;return-c/2*((t-=2)*t*t*t-2)+b;},easeInQuint:function(t,b,c,d){return c*(t/=d)*t*t*t*t+b;},easeOutQuint:function(t,b,c,d){return c*((t=t/d-1)*t*t*t*t+1)+b;},easeInOutQuint:function(t,b,c,d){if((t/=d/2)<1)return c/2*t*t*t*t*t+b;return c/2*((t-=2)*t*t*t*t+2)+b;},easeInSine:function(t,b,c,d){return-c*Math.cos(t/d*(3.14/2))+c+b;},easeOutSine:function(t,b,c,d){return c*Math.sin(t/d*(3.14/2))+b;},easeInOutSine:function(t,b,c,d){return-c/2*(Math.cos(3.14*t/d)-1)+b;},easeInExpo:function(t,b,c,d){return(t==0)?b:c*Math.pow(2,10*(t/d-1))+b;},easeOutExpo:function(t,b,c,d){return(t==d)?b+c:c*(-Math.pow(2,-10*t/d)+1)+b;},easeInOutExpo:function(t,b,c,d){if(t==0)return b;if(t==d)return b+c;if((t/=d/2)<1)return c/2*Math.pow(2,10*(t-1))+b;return c/2*(-Math.pow(2,-10*--t)+2)+b;},easeInCirc:function(t,b,c,d){return-c*(Math.sqrt(1-(t/=d)*t)-1)+b;},easeOutCirc:function(t,b,c,d){return c*Math.sqrt(1-(t=t/d-1)*t)+b;},easeInOutCirc:function(t,b,c,d){if((t/=d/2)<1)return-c/2*(Math.sqrt(1-t*t)-1)+b;return c/2*(Math.sqrt(1-(t-=2)*t)+1)+b;},easeInBounce:function(t,b,c,d){return c-dnn.easing.easeOutBounce(d-t,0,c,d)+b;},easeOutBounce:function(t,b,c,d){if((t/=d)<(1/2.75)){return c*(7.5625*t*t)+b;}else if(t<(2/2.75)){return c*(7.5625*(t-=(1.5/2.75))*t+.75)+b;}else if(t<(2.5/2.75)){return c*(7.5625*(t-=(2.25/2.75))*t+.9375)+b;}else{return c*(7.5625*(t-=(2.625/2.75))*t+.984375)+b;}},easeInOutBounce:function(t,b,c,d){if(t<d/2)return dnn.easing.easeInBounce(t*2,0,c,d)*.5+b;return dnn.easing.easeOutBounce(t*2-d,0,c,d)*.5+c*.5+b;}});
