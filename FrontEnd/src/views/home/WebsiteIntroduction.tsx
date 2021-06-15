import React from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

const WebsiteIntroduction: React.FC<{
  className?: string;
  style?: React.CSSProperties;
}> = ({ className, style }) => {
  const { i18n } = useTranslation();

  if (i18n.language.startsWith("zh")) {
    return (
      <div className={className} style={style}>
        <h2>
          欢迎来到<strong>时间线</strong>！🎉🎉🎉
        </h2>
        <p>
          本网站由无数个独立的时间线构成，每一个时间线都是一个消息列表，类似于一个聊天软件（比如QQ）。
        </p>
        <p>
          如果你拥有一个账号，<Link to="/login">登陆</Link>
          后你可以自由地在属于你的时间线中发送内容，支持markdown和上传图片哦！你可以创建一个新的时间线来开启一个新的话题。你也可以设置相关权限，只让一部分人能看到时间线的内容。
        </p>
        <p>
          如果你没有账号，那么你可以去浏览一下公开的时间线，比如下面这些站长设置的高光时间线。
        </p>
        <p>
          鉴于这个网站在我的小型服务器上部署，所以没有开放注册。如果你也想把这个服务部署到自己的服务器上，你可以在
          <Link to="/about">关于</Link>页面找到一些信息。
        </p>
        <p>
          <small className="text-secondary">
            这一段介绍是我的对象抱怨多次我的网站他根本看不明白之后加的，希望你能顺利看懂这个网站的逻辑！😅
          </small>
        </p>
      </div>
    );
  } else {
    return (
      <div className={className} style={style}>
        <h2>
          Welcome to <strong>Timeline</strong>!🎉🎉🎉
        </h2>
        <p>
          This website consists of many individual timelines. Each timeline is a
          list of messages just like a chat app.
        </p>
        <p>
          If you do have an account, you can <Link to="/login">login</Link> and
          post messages, which supports Markdown and images, in your timelines.
          You can also create a new timeline to open a new topic. You can set
          the permission of a timeline to only allow specified people to see the
          content of the timeline.
        </p>
        <p>
          If you don&apos;t have an account, you can view some public timelines
          like highlight timelines below set by website manager.
        </p>
        <p>
          Since this website is hosted on my tiny server, so account registry is
          not opened. If you want to host this service on your own server, you
          can find some useful information on <Link to="/about">about</Link>{" "}
          page.
        </p>
        <p>
          <small className="text-secondary">
            This introduction is added after my lover complained a lot of times
            about the obscuration of my website. May you understand the logic of
            it!😅
          </small>
        </p>
      </div>
    );
  }
};

export default WebsiteIntroduction;
